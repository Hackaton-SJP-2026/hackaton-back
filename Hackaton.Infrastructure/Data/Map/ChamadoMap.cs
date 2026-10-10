using Hackaton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hackaton.Infrastructure.Data.Map;

public sealed class ChamadoMap : IEntityTypeConfiguration<Chamado>
{
    public void Configure(EntityTypeBuilder<Chamado> builder)
    {
        builder.ToTable("Chamados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descricao).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.AreaAtendimento).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Natureza).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Modalidade).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Prioridade).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.EtapaAtendimento).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Equipamento).HasMaxLength(160);
        builder.Property(x => x.Contato).HasMaxLength(100);
        builder.Property(x => x.LocalRetiradaMaterial).HasMaxLength(300);
        builder.HasIndex(x => x.SolicitanteId);
        builder.HasIndex(x => x.UnidadeAtendimentoId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TecnicoResponsavelId);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.SolicitanteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UnidadeAtendimento>().WithMany().HasForeignKey(x => x.UnidadeAtendimentoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Tecnico>().WithMany().HasForeignKey(x => x.TecnicoResponsavelId).OnDelete(DeleteBehavior.Restrict);
    }
}
