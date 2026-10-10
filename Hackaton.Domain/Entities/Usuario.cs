namespace Hackaton.Domain.Entities;

public sealed class Usuario : EntityAudited
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public Guid PrefeituraId { get; set; }
    public string Papel { get; set; } = "Usuario";
    public bool Ativo { get; set; } = true;
}
