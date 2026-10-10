namespace Hackaton.Domain.Entities;

public sealed class AnexoChamado : EntityAudited
{
    public Guid ChamadoId { get; set; }
    public Guid AutorUsuarioId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public string Localizacao { get; set; } = string.Empty;
    public string? ConteudoBase64 { get; set; }
    public string TipoAnexo { get; set; } = "Evidencia";
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
