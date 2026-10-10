namespace Hackaton.Domain.Entities;

public sealed class Prefeitura : EntityAudited
{
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
}
