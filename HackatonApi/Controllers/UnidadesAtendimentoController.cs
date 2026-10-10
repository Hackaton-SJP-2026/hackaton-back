using Hackaton.Application.DTOs;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HackatonApi.Controllers;

public sealed class UnidadesAtendimentoController : BaseController<UnidadeAtendimento, UnidadeAtendimentoDto>
{
    private readonly AppDbContext _db;
    public UnidadesAtendimentoController(IAppService<UnidadeAtendimento, UnidadeAtendimentoDto> service, AppDbContext db) : base(service) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        var data = await _db.UnidadesAtendimento.AsNoTracking().Where(x => x.PrefeituraId == tenantId)
            .Select(x => new UnidadeAtendimentoDto { Id = x.Id, Nome = x.Nome, PrefeituraId = x.PrefeituraId,
                Endereco = x.Endereco, Latitude = x.Latitude, Longitude = x.Longitude, Ativa = x.Ativa })
            .ToListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        var unit = await _db.UnidadesAtendimento.AsNoTracking().Where(x => x.Id == id && x.PrefeituraId == tenantId)
            .Select(x => new UnidadeAtendimentoDto { Id = x.Id, Nome = x.Nome, PrefeituraId = x.PrefeituraId,
                Endereco = x.Endereco, Latitude = x.Latitude, Longitude = x.Longitude, Ativa = x.Ativa })
            .SingleOrDefaultAsync(cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpPost]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Post([FromBody] UnidadeAtendimentoDto dto, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (dto.PrefeituraId != tenantId) return Forbid();
        return Ok(await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Put(Guid id, [FromBody] UnidadeAtendimentoDto dto, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (dto.PrefeituraId != tenantId) return Forbid();
        if (!await _db.UnidadesAtendimento.AnyAsync(x => x.Id == id && x.PrefeituraId == tenantId, cancellationToken)) return NotFound();
        return Ok(await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (!await _db.UnidadesAtendimento.AnyAsync(x => x.Id == id && x.PrefeituraId == tenantId, cancellationToken)) return NotFound();
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private bool Tenant(out Guid id) => Guid.TryParse(User.FindFirstValue("prefeitura_id"), out id);
}
