using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hackaton.Application.DTOs;

public sealed record UnidadeAtendimentoDto
{
    [ReadOnly(true)] public Guid Id { get; init; }
    [Required, StringLength(160, MinimumLength = 2)] public string Nome { get; init; } = string.Empty;
    [Required] public Guid PrefeituraId { get; init; }
    [Required, StringLength(300)] public string Endereco { get; init; } = string.Empty;
    [Range(-90, 90)] public double? Latitude { get; init; }
    [Range(-180, 180)] public double? Longitude { get; init; }
    public bool Ativa { get; init; } = true;
}
