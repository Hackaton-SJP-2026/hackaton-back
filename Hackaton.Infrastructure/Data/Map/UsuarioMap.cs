using Hackaton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hackaton.Infrastructure.Data.Map;

public sealed class UsuarioMap : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512);
        builder.HasIndex(x => new { x.PrefeituraId, x.Email }).IsUnique();
        builder.Property(x => x.Telefone).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Papel).HasMaxLength(20).IsRequired();
        builder.HasOne<Prefeitura>().WithMany().HasForeignKey(x => x.PrefeituraId).OnDelete(DeleteBehavior.Restrict);
    }
}
