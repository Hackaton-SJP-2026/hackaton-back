using Hackaton.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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
        var inicio = desde?.ToUniversalTime() ?? new DateTime(fim.Year, fim.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        if (inicio > fim || (fim - inicio).TotalDays > 366) return BadRequest(new { message = "Informe um período válido de até 366 dias." });

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
                           select new { t.Id, u.Nome, FotoUrl = t.FotoUrl }).ToListAsync(cancellationToken);
        var pontuacaoPorTecnico = totais.ToDictionary(x => x.TecnicoId);

        var ranking = nomes
            .Select(t =>
            {
                var pontuacao = pontuacaoPorTecnico.GetValueOrDefault(t.Id);
                var confirmados = pontuacao?.Confirmados ?? 0;
                var provisorios = pontuacao?.Provisorios ?? 0;
                return new RankingTecnicoDto(t.Id, t.Nome, t.FotoUrl, confirmados, provisorios, confirmados + provisorios);
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
            x.AreaAtendimento, x.Natureza, x.Modalidade, x.UnidadeAtendimentoId,
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

public sealed record ChamadoResumoDto(Guid Id, string Descricao, StatusChamado Status, string Prioridade,
    string AreaAtendimento, string Natureza, string Modalidade, Guid UnidadeAtendimentoId,
    string Unidade, Guid SolicitanteId, string Solicitante, Guid? TecnicoResponsavelId,
    DateTime AbertoEm, DateTime? InicioAtendimento, string EtapaAtendimento);

public sealed record RankingTecnicoDto(Guid TecnicoId, string Nome, string? FotoUrl,
    int PontosConfirmados, int PontosProvisorios, int PontosNoPeriodo, int Posicao = 0);

public sealed record EventoProgressoRequest([Required, StringLength(4000, MinimumLength = 3)] string Descricao);
