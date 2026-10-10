namespace Hackaton.Domain.Entities;

public sealed class TecnicoUnidadeAtendimento : EntityAudited
{
    public Guid TecnicoId { get; set; }
    public Guid UnidadeAtendimentoId { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Responsavel { get; set; }
}
