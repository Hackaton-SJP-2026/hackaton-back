namespace Hackaton.Domain.Entities;

public sealed class Tecnico : EntityAudited
{
    public Guid UsuarioId { get; set; }
    public string? FotoUrl { get; set; }
    public string Disponibilidade { get; set; } = "Disponivel";
    public string? MotivoIndisponibilidade { get; set; }
    public DateTime? DisponibilidadeAtualizadaEm { get; set; }
    public int Pontos { get; set; }
    public string Patente { get; set; } = "Soldado raso";
}
