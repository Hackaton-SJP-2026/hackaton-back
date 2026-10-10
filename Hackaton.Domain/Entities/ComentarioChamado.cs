namespace Hackaton.Domain.Entities;

public sealed class ComentarioChamado : EntityAudited
{
    public Guid ChamadoId { get; set; }
    public Guid AutorUsuarioId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
