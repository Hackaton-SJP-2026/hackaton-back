using Microsoft.EntityFrameworkCore;

namespace Hackaton.Infrastructure.Data;

// Mesmo domínio e repositórios; histórico de migrations próprio do MariaDB.
public sealed class MariaDbAppDbContext : AppDbContext
{
    public MariaDbAppDbContext(DbContextOptions<MariaDbAppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasCharSet("utf8mb4");
        // MariaDB não oferece índices filtrados; NULL permite vários colaboradores.
        var vinculo = modelBuilder.Entity<Hackaton.Domain.Entities.TecnicoUnidadeAtendimento>();
        vinculo.HasIndex(x => x.UnidadeAtendimentoId).IsUnique(false).HasFilter(null);
        vinculo.Property<Guid?>("UnidadeResponsavelId")
            .HasComputedColumnSql("CASE WHEN Responsavel = 1 THEN RTRIM(UnidadeAtendimentoId) ELSE NULL END", stored: true);
        vinculo.HasIndex("UnidadeResponsavelId").IsUnique();
        modelBuilder.Entity<Hackaton.Domain.Entities.AnexoChamado>()
            .Property(x => x.ConteudoBase64).HasColumnType("longtext");
        // Um endpoint pode ter 2.000 caracteres e exceder o limite de chave do InnoDB.
        // O índice continua cobrindo todo o valor usando ASCII (URLs devem ser ASCII).
        modelBuilder.Entity<Hackaton.Domain.Entities.DispositivoPush>()
            .Property(x => x.Endpoint).HasCharSet("ascii").UseCollation("ascii_bin");
    }
}
