using Hackaton.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hackaton.Application.DTOs;

public sealed record ChamadoDto
{
    [ReadOnly(true)] public Guid Id { get; init; }
    [Required, StringLength(4000, MinimumLength = 5)] public string Descricao { get; init; } = string.Empty;
    [Required] public Guid SolicitanteId { get; init; }
    [Required] public Guid UnidadeAtendimentoId { get; init; }
    [Required, RegularExpression("^(Suporte|Telecom|TI|TI / Sistemas)$")] public string AreaAtendimento { get; init; } = string.Empty;
    [Required, RegularExpression("^(Hardware|Software)$")] public string Natureza { get; init; } = string.Empty;
    [Required, RegularExpression("^(Remoto|Presencial)$")] public string Modalidade { get; init; } = string.Empty;
    [Required, RegularExpression("^(Baixa|Normal|Alta|Urgente)$")] public string Prioridade { get; init; } = "Normal";
    [ReadOnly(true)] public StatusChamado Status { get; init; } = StatusChamado.Aberto;
    [ReadOnly(true)] public string EtapaAtendimento { get; init; } = "Aberto";
    [StringLength(160)] public string? Equipamento { get; init; }
    [StringLength(100)] public string? Contato { get; init; }
    [Range(1, 100000)] public int PessoasAfetadas { get; init; } = 1;
    public bool ExisteAlternativa { get; init; }
    public bool NecessitaMaterial { get; init; }
    [StringLength(300)] public string? LocalRetiradaMaterial { get; init; }
    [ReadOnly(true)] public Guid? TecnicoResponsavelId { get; init; }
    [ReadOnly(true)] public DateTime? InicioAtendimento { get; init; }
}
