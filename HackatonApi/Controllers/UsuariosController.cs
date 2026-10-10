using Hackaton.Application.DTOs;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HackatonApi.Controllers;

public sealed class UsuariosController : BaseController<Usuario, UsuarioDto>
{
    private readonly AppDbContext _db;
    public UsuariosController(IAppService<Usuario, UsuarioDto> service, AppDbContext db) : base(service) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        var data = await _db.Usuarios.AsNoTracking().Where(x => x.PrefeituraId == tenantId)
            .Select(x => new UsuarioDto { Id = x.Id, Nome = x.Nome, Email = x.Email, Telefone = x.Telefone,
                PrefeituraId = x.PrefeituraId, Papel = x.Papel, Ativo = x.Ativo })
            .ToListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        var user = await _db.Usuarios.AsNoTracking().Where(x => x.Id == id && x.PrefeituraId == tenantId)
            .Select(x => new UsuarioDto { Id = x.Id, Nome = x.Nome, Email = x.Email, Telefone = x.Telefone,
                PrefeituraId = x.PrefeituraId, Papel = x.Papel, Ativo = x.Ativo })
            .SingleOrDefaultAsync(cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Post([FromBody] UsuarioDto dto, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (dto.PrefeituraId != tenantId || (User.IsInRole("Gestor") && (dto.Papel is "Admin" or "Gestor"))) return Forbid();
        return Ok(await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Put(Guid id, [FromBody] UsuarioDto dto, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (dto.PrefeituraId != tenantId || (User.IsInRole("Gestor") && (dto.Papel is "Admin" or "Gestor"))) return Forbid();
        if (!await _db.Usuarios.AnyAsync(x => x.Id == id && x.PrefeituraId == tenantId, cancellationToken)) return NotFound();
        return Ok(await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Gestor,Admin")]
    public override async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!Tenant(out var tenantId)) return Forbid();
        if (!await _db.Usuarios.AnyAsync(x => x.Id == id && x.PrefeituraId == tenantId, cancellationToken)) return NotFound();
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private bool Tenant(out Guid id) => Guid.TryParse(User.FindFirstValue("prefeitura_id"), out id);
}
