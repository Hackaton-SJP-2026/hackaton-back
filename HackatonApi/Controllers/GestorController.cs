using Hackaton.Domain.Enums;
using System.Security.Claims;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackatonApi.Controllers;

[ApiController]
[Route("api/gestor")]
[Authorize(Roles = "Gestor,Admin")]
public sealed class GestorController : ControllerBase
{
    private readonly AppDbContext _db;
    public GestorController(AppDbContext db) => _db = db;

    [HttpGet("chamados")]
    public async Task<IActionResult> ListarChamados([FromQuery] string? status, [FromQuery] string? prioridade,
        [FromQuery] string? area, [FromQuery] Guid? unidadeId, [FromQuery] Guid? tecnicoId,
        [FromQuery] Guid? solicitanteId, [FromQuery] DateTime? desde, [FromQuery] DateTime? ate,
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50, CancellationToken cancellationToken = default)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        if (pagina < 1 || tamanhoPagina is < 1 or > 200) return BadRequest(new { message = "Paginação inválida." });
        if (desde.HasValue && ate.HasValue && desde > ate) return BadRequest(new { message = "O intervalo de datas é inválido." });

        var query = TicketsDaPrefeitura(prefeituraId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!StatusChamadoTexto.TryParse(status, out var statusChamado))
                return BadRequest(new { message = "Status inválido. Use Aberto, Em Análise ou Resolvido." });
            query = query.Where(x => x.Status == statusChamado);
        }
        if (!string.IsNullOrWhiteSpace(prioridade)) query = query.Where(x => x.Prioridade == prioridade);
        if (!string.IsNullOrWhiteSpace(area)) query = query.Where(x => x.AreaAtendimento == area);
        if (unidadeId.HasValue) query = query.Where(x => x.UnidadeAtendimentoId == unidadeId);
        if (tecnicoId.HasValue) query = query.Where(x => x.TecnicoResponsavelId == tecnicoId);
        if (solicitanteId.HasValue) query = query.Where(x => x.SolicitanteId == solicitanteId);
        if (desde.HasValue) query = query.Where(x => x.CreationTime >= desde.Value.ToUniversalTime());
        if (ate.HasValue) query = query.Where(x => x.CreationTime <= ate.Value.ToUniversalTime());

        var total = await query.CountAsync(cancellationToken);
        var tickets = await query.OrderByDescending(x => x.CreationTime)
            .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync(cancellationToken);
        return Ok(new { Pagina = pagina, TamanhoPagina = tamanhoPagina, Total = total, Chamados = await Projetar(tickets, cancellationToken) });
    }

    [HttpGet("tecnicos/posicoes/atuais")]
    public async Task<IActionResult> PosicoesAtuais(CancellationToken cancellationToken)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        var tecnicos = await (from t in _db.Tecnicos.AsNoTracking()
                              join u in _db.Usuarios.AsNoTracking() on t.UsuarioId equals u.Id
                              where u.PrefeituraId == prefeituraId && u.Ativo
                              select new { TecnicoId = t.Id, t.FotoUrl, t.Disponibilidade, UsuarioId = u.Id, u.Nome })
            .ToListAsync(cancellationToken);
        var ids = tecnicos.Select(t => t.TecnicoId).ToArray();
        var positions = new List<PosicaoTecnico>();
        if (ids.Length > 0)
        {
            var latestTimes = _db.PosicoesTecnico.AsNoTracking().Where(p => ids.Contains(p.TecnicoId))
                .GroupBy(p => p.TecnicoId).Select(g => new { TecnicoId = g.Key, DataHora = g.Max(x => x.DataHora) });
            positions = await (from p in _db.PosicoesTecnico.AsNoTracking()
                               join latest in latestTimes on new { p.TecnicoId, p.DataHora } equals new { latest.TecnicoId, latest.DataHora }
                               select p).ToListAsync(cancellationToken);
        }
        var latestByTech = positions.GroupBy(x => x.TecnicoId).ToDictionary(x => x.Key, x => x.OrderByDescending(p => p.Id).First());
        var result = tecnicos.Select(t =>
        {
            latestByTech.TryGetValue(t.TecnicoId, out var p);
            return new PosicaoAtualDto(t.TecnicoId, t.UsuarioId, t.Nome, t.FotoUrl, t.Disponibilidade,
                p?.Latitude, p?.Longitude, p?.PrecisaoMetros, p?.DataHora);
        }).ToList();
        return Ok(result);
    }

    [HttpGet("chamados/{chamadoId:guid}/anexos")]
    public async Task<IActionResult> AnexosChamado(Guid chamadoId, CancellationToken cancellationToken)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        var pertenceAoTenant = await _db.Chamados.AsNoTracking().AnyAsync(c => c.Id == chamadoId
            && _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId), cancellationToken);
        if (!pertenceAoTenant) return NotFound();
        var anexos = await _db.AnexosChamado.AsNoTracking().Where(x => x.ChamadoId == chamadoId)
            .OrderBy(x => x.DataHora).Select(x => new
            {
                x.Id, x.NomeArquivo, x.TipoConteudo, x.TamanhoBytes, x.TipoAnexo, x.DataHora, x.ConteudoBase64
            }).ToListAsync(cancellationToken);
        return Ok(anexos);
    }

    [HttpGet("tecnicos/{tecnicoId:guid}/posicoes")]
    public async Task<IActionResult> HistoricoPosicoes(Guid tecnicoId, [FromQuery] int limite = 100,
        CancellationToken cancellationToken = default)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        if (limite is < 1 or > 1000) return BadRequest(new { message = "Limite deve estar entre 1 e 1000." });
        if (!await (from t in _db.Tecnicos.AsNoTracking()
                    join u in _db.Usuarios.AsNoTracking() on t.UsuarioId equals u.Id
                    where t.Id == tecnicoId && u.PrefeituraId == prefeituraId
                    select t.Id).AnyAsync(cancellationToken)) return NotFound();

        var data = await _db.PosicoesTecnico.AsNoTracking().Where(x => x.TecnicoId == tecnicoId)
            .OrderByDescending(x => x.DataHora).Take(limite)
            .Select(x => new { x.Id, x.ChamadoId, x.Latitude, x.Longitude, x.PrecisaoMetros, x.DataHora })
            .ToListAsync(cancellationToken);
        return Ok(data);
    }

    private IQueryable<Chamado> TicketsDaPrefeitura(Guid prefeituraId)
        => _db.Chamados.AsNoTracking().Where(c => _db.UnidadesAtendimento
            .Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId));

    private async Task<List<ChamadoResumoDto>> Projetar(List<Chamado> tickets, CancellationToken cancellationToken)
    {
        var unitIds = tickets.Select(x => x.UnidadeAtendimentoId).Distinct().ToArray();
        var userIds = tickets.Select(x => x.SolicitanteId).Distinct().ToArray();
        var units = await _db.UnidadesAtendimento.AsNoTracking().Where(x => unitIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Nome, cancellationToken);
        var users = await _db.Usuarios.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Nome, cancellationToken);
        return tickets.Select(x => new ChamadoResumoDto(x.Id, x.Descricao, x.Status, x.Prioridade,
            x.NivelComplexidade, x.Pontos, x.AreaAtendimento, x.Natureza, x.Modalidade, x.UnidadeAtendimentoId,
            units.GetValueOrDefault(x.UnidadeAtendimentoId, ""), x.SolicitanteId,
            users.GetValueOrDefault(x.SolicitanteId, ""), x.TecnicoResponsavelId,
            x.CreationTime, x.InicioAtendimento, x.EtapaAtendimento)).ToList();
    }

    private bool Tenant(out Guid prefeituraId)
        => Guid.TryParse(User.FindFirstValue("prefeitura_id"), out prefeituraId);
}

public sealed record PosicaoAtualDto(Guid TecnicoId, Guid UsuarioId, string Nome,
    string? FotoUrl, string Disponibilidade, double? Latitude, double? Longitude,
    double? PrecisaoMetros, DateTime? DataHora);
