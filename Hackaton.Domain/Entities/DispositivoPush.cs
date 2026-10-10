namespace Hackaton.Domain.Entities;

public sealed class DispositivoPush : EntityAudited
{
    public Guid UsuarioId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string ChavePublica { get; set; } = string.Empty;
    public string ChaveAutenticacao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime? UltimoUsoEm { get; set; }
}
