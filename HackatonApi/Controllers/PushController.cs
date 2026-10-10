using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hackaton.Application.Interface;
using HackatonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackatonApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PushController : ControllerBase
{
    private readonly IPushNotificationService _pushNotificationService;

    public PushController(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    [HttpPost("dispositivos")]
    [Authorize]
    public async Task<IActionResult> RegistrarDispositivo([FromBody] RegistrarDispositivoRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Forbid();

        if (request.UsuarioId != usuarioId)
            return Forbid();

        await _pushNotificationService.RegisterDeviceAsync(
            request.UsuarioId,
            request.Endpoint,
            request.ChavePublica,
            request.ChaveAutenticacao,
            cancellationToken);

        return Ok(new { ok = true });
    }
}

public sealed record RegistrarDispositivoRequest(
    [Required] Guid UsuarioId,
    [Required] string Endpoint,
    [Required] string ChavePublica,
    [Required] string ChaveAutenticacao);
