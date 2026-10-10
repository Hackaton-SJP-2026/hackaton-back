namespace Hackaton.Domain.Entities;

public sealed class PosicaoTecnico : Entity
{
    public Guid TecnicoId { get; set; }
    public Guid? ChamadoId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? PrecisaoMetros { get; set; }
    public DateTime DataHora { get; set; }
}
