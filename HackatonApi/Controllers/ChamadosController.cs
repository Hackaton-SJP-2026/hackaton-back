using Hackaton.Application.DTOs;
using Hackaton.Application.Exceptions;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HackatonApi.Controllers;

public sealed class ChamadosController : BaseController<Chamado, ChamadoDto>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContext;

    public ChamadosController(IAppService<Chamado, ChamadoDto> service, AppDbContext db, IHttpContextAccessor httpContext) : base(service)
    { _db = db; _httpContext = httpContext; }

    [HttpGet]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        return Ok(await _db.Chamados.AsNoTracking().Where(c => _db.UnidadesAtendimento
            .Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == tenantId)).ToListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        var chamado = await _db.Chamados.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id
            && _db.UnidadesAtendimento.Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == tenantId), cancellationToken);
        return chamado is null ? NotFound() : Ok(chamado);
    }

    [HttpPost]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Post([FromBody] ChamadoDto dto, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (!await UnidadeDaPrefeitura(dto.UnidadeAtendimentoId, tenantId, cancellationToken)) return NotFound();
        try { return Ok(await _service.CreateAsync(dto, cancellationToken)); }
        catch (EntityValidationException ex) { return BadRequest(new { errors = ex.Errors }); }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Put(Guid id, [FromBody] ChamadoDto dto, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (!await _db.Chamados.AsNoTracking().AnyAsync(c => c.Id == id && _db.UnidadesAtendimento
                .Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == tenantId), cancellationToken)) return NotFound();
        if (!await UnidadeDaPrefeitura(dto.UnidadeAtendimentoId, tenantId, cancellationToken)) return NotFound();
        try { return Ok(await _service.UpdateAsync(id, dto, cancellationToken)); }
        catch (EntityValidationException ex) { return BadRequest(new { errors = ex.Errors }); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (!await _db.Chamados.AsNoTracking().AnyAsync(c => c.Id == id && _db.UnidadesAtendimento
                .Any(u => u.Id == c.UnidadeAtendimentoId && u.PrefeituraId == tenantId), cancellationToken)) return NotFound();
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private bool Tenant(out Guid id)
        => Guid.TryParse(_httpContext.HttpContext?.User.FindFirstValue("prefeitura_id"), out id);

    private Task<bool> UnidadeDaPrefeitura(Guid unidadeId, Guid tenantId, CancellationToken cancellationToken)
        => _db.UnidadesAtendimento.AsNoTracking().AnyAsync(u => u.Id == unidadeId && u.PrefeituraId == tenantId, cancellationToken);
}
