using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hackaton.Application.DTOs;

public sealed record UsuarioDto
{
    [ReadOnly(true)] public Guid Id { get; init; }
    [Required, StringLength(160, MinimumLength = 2)] public string Nome { get; init; } = string.Empty;
    [Required, EmailAddress, StringLength(254)] public string Email { get; init; } = string.Empty;
    [StringLength(30)] public string Telefone { get; init; } = string.Empty;
    [Required] public Guid PrefeituraId { get; init; }
    [Required, RegularExpression("^(Usuario|Tecnico|Admin)$")] public string Papel { get; init; } = "Usuario";
    public bool Ativo { get; init; } = true;
}
