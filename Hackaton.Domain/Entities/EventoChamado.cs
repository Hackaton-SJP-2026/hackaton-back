namespace Hackaton.Domain.Entities;

public sealed class EventoChamado : EntityAudited
{
    public Guid ChamadoId { get; set; }
    public Guid? AutorUsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
