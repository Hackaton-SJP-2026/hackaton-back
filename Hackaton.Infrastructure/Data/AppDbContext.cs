using Microsoft.EntityFrameworkCore;
using Hackaton.Domain.Entities;

namespace Hackaton.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<UnidadeAtendimento> UnidadesAtendimento => Set<UnidadeAtendimento>();
        public DbSet<Chamado> Chamados => Set<Chamado>();
        public DbSet<Prefeitura> Prefeituras => Set<Prefeitura>();
        public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
        public DbSet<TecnicoUnidadeAtendimento> TecnicoUnidadesAtendimento => Set<TecnicoUnidadeAtendimento>();
        public DbSet<EventoChamado> EventosChamado => Set<EventoChamado>();
        public DbSet<ComentarioChamado> ComentariosChamado => Set<ComentarioChamado>();
        public DbSet<ParticipanteChamado> ParticipantesChamado => Set<ParticipanteChamado>();
        public DbSet<PosicaoTecnico> PosicoesTecnico => Set<PosicaoTecnico>();
        public DbSet<NotificacaoPush> NotificacoesPush => Set<NotificacaoPush>();
        public DbSet<DispositivoPush> DispositivosPush => Set<DispositivoPush>();
        public DbSet<AnexoChamado> AnexosChamado => Set<AnexoChamado>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
