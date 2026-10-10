using Hackaton.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackatonApi.Controllers;

[ApiController]
[Route("api/app")]
[Authorize]
public sealed class AppTecnicoController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    public AppTecnicoController(AppDbContext db, IUnitOfWork unitOfWork) { _db = db; _unitOfWork = unitOfWork; }

    [HttpGet("chamados/{categoria}")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> Chamados(string categoria, CancellationToken cancellationToken)
    {
        if (!Identidade(out _, out var tecnicoId, out var prefeituraId)) return Forbid();

        var query = _db.Chamados.AsNoTracking().Where(c =>
            _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId));

        switch (categoria.ToLowerInvariant())
        {
            case "abertos":
                query = query.Where(c => c.Status == StatusChamado.Aberto && c.TecnicoResponsavelId == null
                    && _db.TecnicoUnidadesAtendimento.Any(v => v.TecnicoId == tecnicoId && v.UnidadeAtendimentoId == c.UnidadeAtendimentoId && v.Ativo));
                break;
            case "pendentes":
                query = query.Where(c => c.TecnicoResponsavelId == tecnicoId && c.Status == StatusChamado.EmAnalise && c.EtapaAtendimento == "Vinculado");
                break;
            case "em-andamento":
                query = query.Where(c => c.Status == StatusChamado.EmAnalise && (c.EtapaAtendimento == "Em andamento" || c.EtapaAtendimento == "Pausado" || c.EtapaAtendimento == "Bloqueado"))
                    .Where(c => c.TecnicoResponsavelId == tecnicoId || _db.ParticipantesChamado.Any(p => p.ChamadoId == c.Id && p.TecnicoId == tecnicoId && p.DataDesvinculo == null));
                break;
            default:
                return BadRequest(new { message = "Categoria inválida. Use abertos, pendentes ou em-andamento." });
        }

        return Ok(await ProjetarChamados(query, cancellationToken));
    }

    [HttpGet("ranking")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> Ranking([FromQuery] DateTime? desde, [FromQuery] DateTime? ate, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("prefeitura_id"), out var prefeituraId)) return Forbid();
        var fim = ate?.ToUniversalTime() ?? DateTime.UtcNow;
        var periodoInformado = desde.HasValue || ate.HasValue;
        var inicio = desde?.ToUniversalTime() ?? (periodoInformado
            ? new DateTime(fim.Year, fim.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            : DateTime.MinValue);
        if (periodoInformado && (inicio > fim || (fim - inicio).TotalDays > 366))
            return BadRequest(new { message = "Informe um período válido de até 366 dias." });

        var totais = await _db.LancamentosPontosTecnico.AsNoTracking()
            .Where(p => p.DataHora >= inicio && p.DataHora <= fim
                && _db.Tecnicos.Any(t => t.Id == p.TecnicoId && _db.Usuarios.Any(u => u.Id == t.UsuarioId && u.PrefeituraId == prefeituraId)))
            .GroupBy(p => p.TecnicoId)
            .Select(g => new
            {
                TecnicoId = g.Key,
                Confirmados = g.Where(p => p.Situacao == "Confirmado").Sum(p => (int?)p.Pontos) ?? 0,
                Provisorios = g.Where(p => p.Situacao == "Provisorio").Sum(p => (int?)p.Pontos) ?? 0
            }).ToListAsync(cancellationToken);

        var nomes = await (from t in _db.Tecnicos.AsNoTracking()
                           join u in _db.Usuarios.AsNoTracking() on t.UsuarioId equals u.Id
                           where u.PrefeituraId == prefeituraId
                           select new { t.Id, u.Nome, FotoUrl = t.FotoUrl, t.Pontos, t.Patente }).ToListAsync(cancellationToken);
        var pontuacaoPorTecnico = totais.ToDictionary(x => x.TecnicoId);

        var ranking = nomes
            .Select(t =>
            {
                var pontuacao = pontuacaoPorTecnico.GetValueOrDefault(t.Id);
                var confirmados = pontuacao?.Confirmados ?? 0;
                var provisorios = pontuacao?.Provisorios ?? 0;
                var progressoPatente = ProgressaoPatenteTecnicoCalculator.Calcular(t.Pontos);
                return new RankingTecnicoDto(t.Id, t.Nome, t.FotoUrl, confirmados, provisorios,
                    confirmados + provisorios, t.Pontos, progressoPatente.Patente.Nome,
                    progressoPatente.Patente.Simbolo, progressoPatente.Proxima?.Nome,
                    progressoPatente.Faltam, progressoPatente.ProgressoPercentual,
                    EstiloPatente(progressoPatente.Patente.Nome));
            })
            .OrderByDescending(x => x.PontosConfirmados).ThenByDescending(x => x.PontosProvisorios)
            .Select((x, index) => x with { Posicao = index + 1 }).ToList();
        return Ok(new { Desde = inicio, Ate = fim, Ranking = ranking });
    }

    [HttpPost("posicoes")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> RegistrarPosicao(PosicaoRequest request, CancellationToken cancellationToken)
    {
        if (!Identidade(out _, out var tecnicoId, out var prefeituraId)) return Forbid();
        if (request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180
            || request.PrecisaoMetros is < 0 or > 10000) return BadRequest(new { message = "Coordenadas ou precisão inválidas." });
        if (!request.ChamadoId.HasValue || !await _db.Chamados.AsNoTracking().AnyAsync(c => c.Id == request.ChamadoId.Value
                && c.Status == StatusChamado.EmAnalise && c.EtapaAtendimento == "Em andamento"
                && (c.TecnicoResponsavelId == tecnicoId || _db.ParticipantesChamado.Any(p => p.ChamadoId == c.Id && p.TecnicoId == tecnicoId && p.DataDesvinculo == null))
                && _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId), cancellationToken))
            return NotFound();

        var posicao = new PosicaoTecnico
        {
            Id = Guid.NewGuid(), TecnicoId = tecnicoId, ChamadoId = request.ChamadoId,
            Latitude = request.Latitude, Longitude = request.Longitude,
            PrecisaoMetros = request.PrecisaoMetros, DataHora = DateTime.UtcNow
        };
        _db.PosicoesTecnico.Add(posicao);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"/api/gestor/tecnicos/{tecnicoId}/posicoes", new { posicao.Id, posicao.DataHora });
    }

    [HttpPost("chamados/{chamadoId:guid}/finalizar")]
    [Authorize(Roles = "Tecnico")]
    [RequestSizeLimit(12_000_000)]
    public async Task<IActionResult> FinalizarChamado(Guid chamadoId, FinalizarChamadoRequest request,
        CancellationToken cancellationToken)
    {
        if (!Identidade(out var usuarioId, out var tecnicoId, out var prefeituraId)) return Forbid();
        if (request.FotoBase64.Length > 11_500_000)
            return BadRequest(new { message = "A foto excede o limite de 8 MB." });

        if (!await _db.Chamados.AnyAsync(c => c.Id == chamadoId
                && c.TecnicoResponsavelId == tecnicoId
                && c.Status == StatusChamado.EmAnalise && c.EtapaAtendimento == "Em andamento"
                && _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId),
                cancellationToken))
            return NotFound(new { message = "Chamado em andamento não encontrado para este técnico." });

        var conteudoBase64 = request.FotoBase64.Trim();
        var tipoConteudo = request.TipoConteudo.Trim().ToLowerInvariant();
        if (conteudoBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var separador = conteudoBase64.IndexOf(',');
            if (separador < 0 || !conteudoBase64[..separador].EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Formato de data URI inválido." });
            var tipoDataUri = conteudoBase64[5..conteudoBase64.IndexOf(';')].ToLowerInvariant();
            if (tipoConteudo != tipoDataUri)
                return BadRequest(new { message = "TipoConteudo deve corresponder ao tipo declarado na foto." });
            conteudoBase64 = conteudoBase64[(separador + 1)..];
        }

        if (tipoConteudo is not ("image/jpeg" or "image/png" or "image/webp"))
            return BadRequest(new { message = "Envie uma imagem JPEG, PNG ou WEBP." });

        byte[] imagem;
        try { imagem = Convert.FromBase64String(conteudoBase64); }
        catch (FormatException) { return BadRequest(new { message = "FotoBase64 inválido." }); }
        if (imagem.Length == 0 || imagem.Length > 8 * 1024 * 1024 || !AssinaturaImagemValida(imagem, tipoConteudo))
            return BadRequest(new { message = "A foto está vazia, excede 8 MB ou não corresponde ao tipo informado." });

        var chamado = await _db.Chamados.SingleAsync(c => c.Id == chamadoId, cancellationToken);
        var tecnico = await _db.Tecnicos.SingleAsync(t => t.Id == tecnicoId, cancellationToken);
        var agora = DateTime.UtcNow;
        var extensao = tipoConteudo switch { "image/jpeg" => "jpg", "image/png" => "png", _ => "webp" };
        var anexo = new AnexoChamado
        {
            Id = Guid.NewGuid(), ChamadoId = chamadoId, AutorUsuarioId = usuarioId,
            NomeArquivo = $"foto-entrega.{extensao}", TipoConteudo = tipoConteudo,
            TamanhoBytes = imagem.Length, Localizacao = "banco-de-dados",
            ConteudoBase64 = Convert.ToBase64String(imagem), TipoAnexo = "FotoEntrega", DataHora = agora
        };
        chamado.EtapaAtendimento = "Aguardando validação";
        _db.AnexosChamado.Add(anexo);
        _db.EventosChamado.Add(new EventoChamado
        {
            Id = Guid.NewGuid(), ChamadoId = chamadoId, AutorUsuarioId = usuarioId,
            Tipo = "ConclusaoTecnica", Descricao = string.IsNullOrWhiteSpace(request.Descricao)
                ? "Técnico finalizou o atendimento e enviou a foto de entrega; aguardando validação do solicitante."
                : request.Descricao.Trim(), DataHora = agora
        });

        var chavePontos = $"conclusao-tecnica:{chamado.Id}:{tecnico.Id}";
        if (!await _db.LancamentosPontosTecnico.AnyAsync(x => x.ChaveIdempotencia == chavePontos, cancellationToken))
        {
            tecnico.Pontos = checked(tecnico.Pontos + chamado.Pontos);
            tecnico.Patente = ProgressaoPatenteTecnicoCalculator.Calcular(tecnico.Pontos).Patente.Nome;
            _db.LancamentosPontosTecnico.Add(new LancamentoPontoTecnico
            {
                Id = Guid.NewGuid(), TecnicoId = tecnico.Id, ChamadoId = chamado.Id,
                Pontos = chamado.Pontos, Situacao = "Provisorio",
                Motivo = "Crédito único pela conclusão técnica do chamado.",
                ChaveIdempotencia = chavePontos, DataHora = agora
            });
        }

        await _unitOfWork.BeginTransaction(cancellationToken);
        try { await _unitOfWork.ConfirmTransaction(cancellationToken); }
        catch
        {
            await _unitOfWork.RejectTransaction(cancellationToken);
            throw;
        }

        var progressoAtual = ProgressaoPatenteTecnicoCalculator.Calcular(tecnico.Pontos);
        return Ok(new
        {
            ChamadoId = chamado.Id, Status = StatusChamadoTexto.Texto(chamado.Status),
            chamado.EtapaAtendimento, AnexoId = anexo.Id, anexo.TipoAnexo,
            anexo.TipoConteudo, anexo.TamanhoBytes, DataHora = agora,
            PontosConcedidos = chamado.Pontos, PontosAcumulados = tecnico.Pontos,
            Patente = progressoAtual.Patente.Nome, SimboloPatente = progressoAtual.Patente.Simbolo,
            ProximaPatente = progressoAtual.Proxima?.Nome, PontosFaltantes = progressoAtual.Faltam,
            ProgressoPercentual = progressoAtual.ProgressoPercentual,
            EstiloTailwind = EstiloPatente(progressoAtual.Patente.Nome)
        });
    }

    [HttpGet("chamados/{chamadoId:guid}/anexos")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> Anexos(Guid chamadoId, CancellationToken cancellationToken)
    {
        if (!Identidade(out _, out var tecnicoId, out var prefeituraId)) return Forbid();
        if (!await TemAcessoAoChamado(chamadoId, tecnicoId, prefeituraId, cancellationToken)) return NotFound();
        var anexos = await _db.AnexosChamado.AsNoTracking().Where(x => x.ChamadoId == chamadoId)
            .OrderBy(x => x.DataHora).Select(x => new
            {
                x.Id, x.NomeArquivo, x.TipoConteudo, x.TamanhoBytes, x.TipoAnexo, x.DataHora, x.ConteudoBase64
            }).ToListAsync(cancellationToken);
        return Ok(anexos);
    }

    private static bool AssinaturaImagemValida(byte[] bytes, string mimeType) => mimeType switch
    {
        "image/jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
        "image/png" => bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        "image/webp" => bytes.Length >= 12 && Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
            && Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP",
        _ => false
    };

    private static string EstiloPatente(string nome) => nome switch
    {
        "Soldado raso" => "bg-slate-100 text-slate-700 border-slate-300",
        "Soldado" => "bg-emerald-100 text-emerald-800 border-emerald-300",
        "Cabo" => "bg-blue-100 text-blue-800 border-blue-300",
        "Sargento" => "bg-cyan-100 text-cyan-800 border-cyan-300",
        "Subtenente" => "bg-violet-100 text-violet-800 border-violet-300",
        "Tenente" => "bg-indigo-100 text-indigo-800 border-indigo-300",
        "Capitão" => "bg-amber-100 text-amber-800 border-amber-300",
        "Major" => "bg-orange-100 text-orange-800 border-orange-300",
        "Coronel" => "bg-rose-100 text-rose-800 border-rose-300",
        "General" => "bg-yellow-100 text-yellow-900 border-yellow-400",
        _ => "bg-slate-100 text-slate-700 border-slate-300"
    };

    [HttpGet("chamados/{chamadoId:guid}/eventos")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> Eventos(Guid chamadoId, CancellationToken cancellationToken)
    {
        if (!Identidade(out _, out var tecnicoId, out var prefeituraId)) return Forbid();
        if (!await TemAcessoAoChamado(chamadoId, tecnicoId, prefeituraId, cancellationToken)) return NotFound();
        var eventos = await (from e in _db.EventosChamado.AsNoTracking()
                             join u in _db.Usuarios.AsNoTracking() on e.AutorUsuarioId equals (Guid?)u.Id into autores
                             from autor in autores.DefaultIfEmpty()
                             where e.ChamadoId == chamadoId orderby e.DataHora
                             select new { e.Id, e.Tipo, e.Descricao, e.DataHora, Autor = autor == null ? "Sistema" : autor.Nome })
            .ToListAsync(cancellationToken);
        return Ok(eventos);
    }

    [HttpPost("chamados/{chamadoId:guid}/eventos")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> AdicionarEvento(Guid chamadoId, EventoProgressoRequest request, CancellationToken cancellationToken)
    {
        if (!Identidade(out var usuarioId, out var tecnicoId, out var prefeituraId)) return Forbid();
        if (!await TemAcessoAoChamado(chamadoId, tecnicoId, prefeituraId, cancellationToken)) return NotFound();
        var evento = new EventoChamado
        {
            ChamadoId = chamadoId, AutorUsuarioId = usuarioId, Tipo = "Atualizacao",
            Descricao = request.Descricao, DataHora = DateTime.UtcNow
        };
        _unitOfWork.Begin(); _db.EventosChamado.Add(evento);
        await _unitOfWork.Confirm(cancellationToken);
        return Created($"/api/app/chamados/{chamadoId}/eventos", new { evento.Id, evento.Tipo, evento.Descricao, evento.DataHora });
    }

    [HttpGet("chamados/{chamadoId:guid}/comentarios")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> Comentarios(Guid chamadoId, CancellationToken cancellationToken)
    {
        if (!Identidade(out _, out var tecnicoId, out var prefeituraId)) return Forbid();
        if (!await TemAcessoAoChamado(chamadoId, tecnicoId, prefeituraId, cancellationToken)) return NotFound();
        var comments = await (from c in _db.ComentariosChamado.AsNoTracking()
                              join u in _db.Usuarios.AsNoTracking() on c.AutorUsuarioId equals u.Id
                              where c.ChamadoId == chamadoId orderby c.DataHora
                              select new { c.Id, c.Texto, c.DataHora, AutorId = c.AutorUsuarioId, Autor = u.Nome })
            .ToListAsync(cancellationToken);
        return Ok(comments);
    }

    [HttpPost("chamados/{chamadoId:guid}/comentarios")]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> AdicionarComentario(Guid chamadoId, ComentarioRequest request, CancellationToken cancellationToken)
    {
        if (!Identidade(out var usuarioId, out var tecnicoId, out var prefeituraId)) return Forbid();
        if (!await TemAcessoAoChamado(chamadoId, tecnicoId, prefeituraId, cancellationToken)) return NotFound();
        var comment = new ComentarioChamado
        {
            ChamadoId = chamadoId, AutorUsuarioId = usuarioId, Texto = request.Texto, DataHora = DateTime.UtcNow
        };
        _unitOfWork.Begin(); _db.ComentariosChamado.Add(comment);
        await _unitOfWork.Confirm(cancellationToken);
        return Created($"/api/app/chamados/{chamadoId}/comentarios", new { comment.Id, comment.Texto, comment.DataHora });
    }

    private Task<bool> TemAcessoAoChamado(Guid chamadoId, Guid tecnicoId, Guid prefeituraId, CancellationToken cancellationToken)
        => _db.Chamados.AsNoTracking().AnyAsync(c => c.Id == chamadoId
            && _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId)
            && (c.TecnicoResponsavelId == tecnicoId || _db.ParticipantesChamado.Any(p => p.ChamadoId == c.Id && p.TecnicoId == tecnicoId && p.DataDesvinculo == null)), cancellationToken);

    private async Task<List<ChamadoResumoDto>> ProjetarChamados(IQueryable<Chamado> query, CancellationToken cancellationToken)
    {
        var tickets = await query.OrderByDescending(x => x.Prioridade == "Urgente")
            .ThenByDescending(x => x.CreationTime).Take(300).ToListAsync(cancellationToken);
        var unitIds = tickets.Select(x => x.UnidadeAtendimentoId).Distinct().ToArray();
        var userIds = tickets.Select(x => x.SolicitanteId).Distinct().ToArray();
        var unidades = await _db.UnidadesAtendimento.AsNoTracking().Where(x => unitIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Nome, cancellationToken);
        var usuarios = await _db.Usuarios.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Nome, cancellationToken);
        return tickets.Select(x => new ChamadoResumoDto(x.Id, x.Descricao, x.Status, x.Prioridade,
            x.NivelComplexidade, x.Pontos, x.AreaAtendimento, x.Natureza, x.Modalidade, x.UnidadeAtendimentoId,
            unidades.GetValueOrDefault(x.UnidadeAtendimentoId, ""), x.SolicitanteId,
            usuarios.GetValueOrDefault(x.SolicitanteId, ""), x.TecnicoResponsavelId,
            x.CreationTime, x.InicioAtendimento, x.EtapaAtendimento)).ToList();
    }

    private bool Identidade(out Guid usuarioId, out Guid tecnicoId, out Guid prefeituraId)
    {
        var usuarioValido = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out usuarioId);
        var tecnicoValido = Guid.TryParse(User.FindFirstValue("tecnico_id"), out tecnicoId);
        var prefeituraValida = Guid.TryParse(User.FindFirstValue("prefeitura_id"), out prefeituraId);
        return usuarioValido && tecnicoValido && prefeituraValida;
    }
}

public sealed record PosicaoRequest([Required] double Latitude, [Required] double Longitude,
    [Range(0, 10000)] double? PrecisaoMetros, [Required] Guid? ChamadoId);

public sealed record FinalizarChamadoRequest(
    [Required, MinLength(1)] string FotoBase64,
    [Required, RegularExpression("^image/(jpeg|png|webp)$")] string TipoConteudo,
    [StringLength(4000, MinimumLength = 3)] string? Descricao = null);

public sealed record ChamadoResumoDto(Guid Id, string Descricao, StatusChamado Status, string Prioridade,
    string NivelComplexidade, int Pontos,
    string AreaAtendimento, string Natureza, string Modalidade, Guid UnidadeAtendimentoId,
    string Unidade, Guid SolicitanteId, string Solicitante, Guid? TecnicoResponsavelId,
    DateTime AbertoEm, DateTime? InicioAtendimento, string EtapaAtendimento);

public sealed record RankingTecnicoDto(Guid TecnicoId, string Nome, string? FotoUrl,
    int PontosConfirmados, int PontosProvisorios, int PontosNoPeriodo,
    int PontosAcumulados, string Patente, string SimboloPatente, string? ProximaPatente,
    int PontosFaltantes, int ProgressoPercentual, string EstiloTailwind, int Posicao = 0);

public sealed record EventoProgressoRequest([Required, StringLength(4000, MinimumLength = 3)] string Descricao);
