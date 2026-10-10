namespace Hackaton.Domain.Entities;

public sealed class Chamado : EntityAudited
{
    public string Descricao { get; set; } = string.Empty;
    public Guid SolicitanteId { get; set; }
    public Guid UnidadeAtendimentoId { get; set; }
    public string AreaAtendimento { get; set; } = string.Empty;
    public string Natureza { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public string Prioridade { get; set; } = "Normal";
    public string Status { get; set; } = "Aberto";
    public string? Equipamento { get; set; }
    public string? Contato { get; set; }
    public int PessoasAfetadas { get; set; } = 1;
    public bool ExisteAlternativa { get; set; }
    public bool NecessitaMaterial { get; set; }
    public string? LocalRetiradaMaterial { get; set; }
    public Guid? TecnicoResponsavelId { get; set; }
    public DateTime? InicioAtendimento { get; set; }
}
