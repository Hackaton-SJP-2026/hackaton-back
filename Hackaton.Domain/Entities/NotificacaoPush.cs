namespace Hackaton.Domain.Entities;

public sealed class NotificacaoPush : Entity
{
    public Guid UsuarioDestinatarioId { get; set; }
    public Guid? ChamadoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente";
    public int Tentativas { get; set; }
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
    public DateTime? EnviadaEm { get; set; }
    public DateTime? ProximaTentativaEm { get; set; }
    public string? ErroEnvio { get; set; }
    public string ChaveDeduplicacao { get; set; } = string.Empty;
}
