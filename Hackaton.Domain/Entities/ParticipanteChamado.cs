namespace Hackaton.Domain.Entities;

public sealed class ParticipanteChamado : EntityAudited
{
    public Guid ChamadoId { get; set; }
    public Guid TecnicoId { get; set; }
    public string Papel { get; set; } = "Colaborador";
    public DateTime DataVinculo { get; set; } = DateTime.UtcNow;
    public DateTime? DataDesvinculo { get; set; }
}
