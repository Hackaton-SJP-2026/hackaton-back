using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HackatonApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(AppDbContext db, IConfiguration configuration, IPasswordHasher<Usuario> passwordHasher, IUnitOfWork unitOfWork)
    {
        _db = db;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    [AllowAnonymous]
    [HttpPost("web/login")]
    public Task<IActionResult> WebLogin(LoginRequest request, CancellationToken cancellationToken)
        => Login(request, appLogin: false, cancellationToken);

    [AllowAnonymous]
    [HttpPost("app/login")]
    public Task<IActionResult> AppLogin(LoginRequest request, CancellationToken cancellationToken)
        => Login(request, appLogin: true, cancellationToken);

    [Authorize(Roles = "Gestor,Admin")]
    [HttpPut("usuarios/{id:guid}/senha")]
    public async Task<IActionResult> DefinirSenha(Guid id, DefinirSenhaRequest request, CancellationToken cancellationToken)
    {
        if (!SenhaValida(request.Senha))
            return BadRequest(new { message = "A senha deve ter ao menos 12 caracteres, incluindo maiúscula, minúscula, número e símbolo." });

        if (!Guid.TryParse(User.FindFirstValue("prefeitura_id"), out var prefeituraId)) return Forbid();
        var usuario = await _db.Usuarios.SingleOrDefaultAsync(x => x.Id == id && x.PrefeituraId == prefeituraId, cancellationToken);
        if (usuario is null) return NotFound();
        if (User.IsInRole("Gestor") && (usuario.Papel is "Admin" or "Gestor")) return Forbid();

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Senha);
        _unitOfWork.Begin();
        await _unitOfWork.Confirm(cancellationToken);
        return NoContent();
    }

    private async Task<IActionResult> Login(LoginRequest request, bool appLogin, CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue<bool>("Authentication:Enabled"))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Autenticação JWT está desabilitada." });

        if (request.PrefeituraId == Guid.Empty || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return Unauthorized(new { message = "Credenciais inválidas." });

        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.AsNoTracking().SingleOrDefaultAsync(
            x => x.PrefeituraId == request.PrefeituraId && x.Email.ToLower() == email && x.Ativo,
            cancellationToken);
        if (usuario?.PasswordHash is null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        var verification = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Senha);
        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Credenciais inválidas." });

        var papelPermitido = appLogin
            ? usuario.Papel == "Tecnico"
            : usuario.Papel is "Usuario" or "Gestor" or "Admin";
        if (!papelPermitido) return Forbid();

        Tecnico? tecnico = null;
        if (appLogin)
        {
            tecnico = await _db.Tecnicos.AsNoTracking().SingleOrDefaultAsync(x => x.UsuarioId == usuario.Id, cancellationToken);
            if (tecnico is null) return Forbid();
        }

        var issuer = _configuration["JwtSettings:Issuer"]!;
        var audience = _configuration["JwtSettings:Audience"]!;
        var key = _configuration["JwtSettings:SigningKey"]!;
        var expires = DateTime.UtcNow.AddMinutes(Math.Clamp(
            _configuration.GetValue<int?>("JwtSettings:AccessTokenMinutes") ?? 480, 5, 1440));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Papel),
            new("prefeitura_id", usuario.PrefeituraId.ToString()),
            new("client", appLogin ? "app" : "web")
        };
        if (tecnico is not null) claims.Add(new Claim("tecnico_id", tecnico.Id.ToString()));

        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: expires, signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));

        return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires,
            usuario.Id, usuario.Nome, usuario.Email, usuario.Papel, usuario.PrefeituraId, tecnico?.Id));
    }

    private static bool SenhaValida(string senha)
        => senha.Length >= 12 && senha.Any(char.IsUpper) && senha.Any(char.IsLower)
            && senha.Any(char.IsDigit) && senha.Any(c => !char.IsLetterOrDigit(c));
}

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(254)] string Email,
    [Required, MaxLength(200)] string Senha,
    [Required] Guid PrefeituraId);

public sealed record DefinirSenhaRequest([Required, MaxLength(200)] string Senha);

public sealed record LoginResponse(string AccessToken, DateTime ExpiraEm, Guid UsuarioId,
    string Nome, string Email, string Papel, Guid PrefeituraId, Guid? TecnicoId);
