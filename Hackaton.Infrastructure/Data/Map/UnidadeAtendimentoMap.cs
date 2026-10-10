using Hackaton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hackaton.Infrastructure.Data.Map;

public sealed class UnidadeAtendimentoMap : IEntityTypeConfiguration<UnidadeAtendimento>
{
    public void Configure(EntityTypeBuilder<UnidadeAtendimento> builder)
    {
        builder.ToTable("UnidadesAtendimento");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Endereco).HasMaxLength(300).IsRequired();
        builder.HasOne<Prefeitura>().WithMany().HasForeignKey(x => x.PrefeituraId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PrefeituraId, x.Nome });
    }
}
