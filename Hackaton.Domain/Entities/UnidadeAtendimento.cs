namespace Hackaton.Domain.Entities;

public sealed class UnidadeAtendimento : EntityAudited
{
    public string Nome { get; set; } = string.Empty;
    public Guid PrefeituraId { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool Ativa { get; set; } = true;
}
