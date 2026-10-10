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
        // Um endpoint pode ter 2.000 caracteres e exceder o limite de chave do InnoDB.
        // O índice continua cobrindo todo o valor usando ASCII (URLs devem ser ASCII).
        modelBuilder.Entity<Hackaton.Domain.Entities.DispositivoPush>()
            .Property(x => x.Endpoint).HasCharSet("ascii").UseCollation("ascii_bin");
    }
}
