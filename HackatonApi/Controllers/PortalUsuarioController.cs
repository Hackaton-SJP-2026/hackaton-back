using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hackaton.Application.DTOs;
using Hackaton.Application.Exceptions;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackatonApi.Controllers;

[ApiController]
[Route("api/usuario")]
[Authorize(Roles = "Usuario")]
public sealed class PortalUsuarioController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAppService<Chamado, ChamadoDto> _chamados;
    private readonly IUnitOfWork _unitOfWork;

    public PortalUsuarioController(AppDbContext db, IAppService<Chamado, ChamadoDto> chamados, IUnitOfWork unitOfWork)
    {
        _db = db;
        _chamados = chamados;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("chamados")]
    public async Task<IActionResult> MeusChamados([FromQuery] string? status, [FromQuery] string? prioridade,
        [FromQuery] Guid? unidadeId, [FromQuery] DateTime? desde, [FromQuery] DateTime? ate,
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50, CancellationToken cancellationToken = default)
    {
        if (!Claims(out var usuarioId, out var prefeituraId)) return Forbid();
        if (pagina < 1 || tamanhoPagina is < 1 or > 200) return BadRequest(new { message = "Paginação inválida." });
        if (desde.HasValue && ate.HasValue && desde > ate) return BadRequest(new { message = "O intervalo de datas é inválido." });
        var query = _db.Chamados.AsNoTracking().Where(c => c.SolicitanteId == usuarioId
            && _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(prioridade)) query = query.Where(x => x.Prioridade == prioridade);
        if (unidadeId.HasValue) query = query.Where(x => x.UnidadeAtendimentoId == unidadeId.Value);
        if (desde.HasValue) query = query.Where(x => x.CreationTime >= desde.Value.ToUniversalTime());
        if (ate.HasValue) query = query.Where(x => x.CreationTime <= ate.Value.ToUniversalTime());
        var total = await query.CountAsync(cancellationToken);
        var tickets = await query.OrderByDescending(x => x.CreationTime).Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina).ToListAsync(cancellationToken);
        var unidadeIds = tickets.Select(x => x.UnidadeAtendimentoId).Distinct().ToArray();
        var unidades = await _db.UnidadesAtendimento.AsNoTracking().Where(x => unidadeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Nome, cancellationToken);
        return Ok(new
        {
            Pagina = pagina, TamanhoPagina = tamanhoPagina, Total = total,
            Chamados = tickets.Select(x => new ChamadoResumoDto(x.Id, x.Descricao, x.Status, x.Prioridade,
                x.AreaAtendimento, x.Natureza, x.Modalidade, x.UnidadeAtendimentoId,
                unidades.GetValueOrDefault(x.UnidadeAtendimentoId, ""), x.SolicitanteId, "Você",
                x.TecnicoResponsavelId, x.CreationTime, x.InicioAtendimento))
        });
    }

    [HttpPost("chamados")]
    public async Task<IActionResult> AbrirChamado(NovoChamadoRequest request, CancellationToken cancellationToken)
    {
        if (!Claims(out var usuarioId, out _)) return Forbid();
        var dto = new ChamadoDto
        {
            Descricao = request.Descricao,
            SolicitanteId = usuarioId,
            UnidadeAtendimentoId = request.UnidadeAtendimentoId,
            AreaAtendimento = request.AreaAtendimento,
            Natureza = request.Natureza,
            Modalidade = request.Modalidade,
            Prioridade = "Normal",
            Equipamento = request.Equipamento,
            Contato = request.Contato,
            PessoasAfetadas = request.PessoasAfetadas,
            ExisteAlternativa = request.ExisteAlternativa,
            NecessitaMaterial = request.NecessitaMaterial,
            LocalRetiradaMaterial = request.LocalRetiradaMaterial
        };
        try
        {
            var criado = await _chamados.CreateAsync(dto, cancellationToken);
            return Created($"/api/usuario/chamados/{criado.Id}", criado);
        }
        catch (EntityValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    [HttpGet("chamados/{chamadoId:guid}")]
    public async Task<IActionResult> Detalhes(Guid chamadoId, CancellationToken cancellationToken)
    {
        if (!Claims(out var usuarioId, out var prefeituraId)) return Forbid();
        if (!await MeuChamado(chamadoId, usuarioId, prefeituraId, cancellationToken)) return NotFound();
        var dto = await _chamados.GetAsync(chamadoId, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("chamados/{chamadoId:guid}/eventos")]
    public async Task<IActionResult> Eventos(Guid chamadoId, CancellationToken cancellationToken)
    {
        if (!Claims(out var userId, out var tenantId)) return Forbid();
        if (!await MeuChamado(chamadoId, userId, tenantId, cancellationToken)) return NotFound();
        var eventos = await (from e in _db.EventosChamado.AsNoTracking()
                             join u in _db.Usuarios.AsNoTracking() on e.AutorUsuarioId equals (Guid?)u.Id into autores
                             from autor in autores.DefaultIfEmpty()
                             where e.ChamadoId == chamadoId
                             orderby e.DataHora
                             select new { e.Id, e.Tipo, e.Descricao, e.DataHora, Autor = autor == null ? "Sistema" : autor.Nome })
            .ToListAsync(cancellationToken);
        return Ok(eventos);
    }

    [HttpGet("chamados/{chamadoId:guid}/comentarios")]
    public async Task<IActionResult> Comentarios(Guid chamadoId, CancellationToken cancellationToken)
    {
        if (!Claims(out var userId, out var tenantId)) return Forbid();
        if (!await MeuChamado(chamadoId, userId, tenantId, cancellationToken)) return NotFound();
        var comments = await (from c in _db.ComentariosChamado.AsNoTracking()
                              join u in _db.Usuarios.AsNoTracking() on c.AutorUsuarioId equals u.Id
                              where c.ChamadoId == chamadoId orderby c.DataHora
                              select new { c.Id, c.Texto, c.DataHora, AutorId = c.AutorUsuarioId, Autor = u.Nome })
            .ToListAsync(cancellationToken);
        return Ok(comments);
    }

    [HttpPost("chamados/{chamadoId:guid}/comentarios")]
    public async Task<IActionResult> AdicionarComentario(Guid chamadoId, ComentarioRequest request, CancellationToken cancellationToken)
    {
        if (!Claims(out var userId, out var tenantId)) return Forbid();
        if (!await MeuChamado(chamadoId, userId, tenantId, cancellationToken)) return NotFound();
        var now = DateTime.UtcNow;
        var comment = new ComentarioChamado { ChamadoId = chamadoId, AutorUsuarioId = userId, Texto = request.Texto, DataHora = now };
        _unitOfWork.Begin();
        _db.ComentariosChamado.Add(comment);
        await _unitOfWork.Confirm(cancellationToken);
        return Created($"/api/usuario/chamados/{chamadoId}/comentarios", new { comment.Id, comment.Texto, comment.DataHora });
    }

    private Task<bool> MeuChamado(Guid chamadoId, Guid userId, Guid prefeituraId, CancellationToken cancellationToken)
        => _db.Chamados.AsNoTracking().AnyAsync(c => c.Id == chamadoId && c.SolicitanteId == userId
            && _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == prefeituraId), cancellationToken);

    private bool Claims(out Guid usuarioId, out Guid prefeituraId)
    {
        var userValid = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out usuarioId);
        var tenantValid = Guid.TryParse(User.FindFirstValue("prefeitura_id"), out prefeituraId);
        return userValid && tenantValid;
    }
}

public sealed record NovoChamadoRequest(
    [Required, StringLength(4000, MinimumLength = 5)] string Descricao,
    [Required] Guid UnidadeAtendimentoId,
    [Required, RegularExpression("^(Suporte|Telecom|TI|TI / Sistemas)$")] string AreaAtendimento,
    [Required, RegularExpression("^(Hardware|Software)$")] string Natureza,
    [Required, RegularExpression("^(Remoto|Presencial)$")] string Modalidade,
    [StringLength(160)] string? Equipamento,
    [StringLength(100)] string? Contato,
    [Range(1, 100000)] int PessoasAfetadas = 1,
    bool ExisteAlternativa = false,
    bool NecessitaMaterial = false,
    [StringLength(300)] string? LocalRetiradaMaterial = null);

public sealed record ComentarioRequest([Required, StringLength(4000, MinimumLength = 1)] string Texto);
