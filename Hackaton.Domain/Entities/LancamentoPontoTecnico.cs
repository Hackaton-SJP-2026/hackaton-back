namespace Hackaton.Domain.Entities;

public sealed class LancamentoPontoTecnico : EntityAudited
{
    public Guid TecnicoId { get; set; }
    public Guid? ChamadoId { get; set; }
    public int Pontos { get; set; }
    public string Situacao { get; set; } = "Provisorio";
    public string Motivo { get; set; } = string.Empty;
    public string ChaveIdempotencia { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
