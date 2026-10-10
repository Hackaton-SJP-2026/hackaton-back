using Hackaton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hackaton.Infrastructure.Data.Map;

public sealed class PrefeituraMap : IEntityTypeConfiguration<Prefeitura>
{
    public void Configure(EntityTypeBuilder<Prefeitura> b)
    {
        b.ToTable("Prefeituras"); b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(180).IsRequired();
        b.Property(x => x.Cnpj).HasMaxLength(14);
        b.Property(x => x.Cidade).HasMaxLength(120).IsRequired();
        b.Property(x => x.Uf).HasMaxLength(2).IsRequired();
        b.HasIndex(x => x.Cnpj).IsUnique();
    }
}

public sealed class TecnicoMap : IEntityTypeConfiguration<Tecnico>
{
    public void Configure(EntityTypeBuilder<Tecnico> b)
    {
        b.ToTable("Tecnicos"); b.HasKey(x => x.Id);
        b.HasOne<Usuario>().WithOne().HasForeignKey<Tecnico>(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.UsuarioId).IsUnique();
        b.Property(x => x.Disponibilidade).HasMaxLength(30).IsRequired();
        b.Property(x => x.FotoUrl).HasMaxLength(1000);
        b.Property(x => x.MotivoIndisponibilidade).HasMaxLength(500);
        b.Property(x => x.Pontos).IsRequired();
        b.Property(x => x.Patente).HasMaxLength(30).IsRequired();
    }
}

public sealed class TecnicoUnidadeAtendimentoMap : IEntityTypeConfiguration<TecnicoUnidadeAtendimento>
{
    public void Configure(EntityTypeBuilder<TecnicoUnidadeAtendimento> b)
    {
        b.ToTable("TecnicoUnidadeAtendimento"); b.HasKey(x => x.Id);
        b.HasOne<Tecnico>().WithMany().HasForeignKey(x => x.TecnicoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<UnidadeAtendimento>().WithMany().HasForeignKey(x => x.UnidadeAtendimentoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TecnicoId, x.UnidadeAtendimentoId }).IsUnique();
        b.HasIndex(x => x.UnidadeAtendimentoId).IsUnique().HasFilter("\"Responsavel\" = 1");
    }
}

public sealed class EventoChamadoMap : IEntityTypeConfiguration<EventoChamado>
{
    public void Configure(EntityTypeBuilder<EventoChamado> b)
    {
        b.ToTable("EventosChamado"); b.HasKey(x => x.Id);
        b.HasOne<Chamado>().WithMany().HasForeignKey(x => x.ChamadoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.AutorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Tipo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Descricao).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => new { x.ChamadoId, x.DataHora });
    }
}

public sealed class ComentarioChamadoMap : IEntityTypeConfiguration<ComentarioChamado>
{
    public void Configure(EntityTypeBuilder<ComentarioChamado> b)
    {
        b.ToTable("ComentariosChamado"); b.HasKey(x => x.Id);
        b.HasOne<Chamado>().WithMany().HasForeignKey(x => x.ChamadoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.AutorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Texto).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => new { x.ChamadoId, x.DataHora });
    }
}

public sealed class ParticipanteChamadoMap : IEntityTypeConfiguration<ParticipanteChamado>
{
    public void Configure(EntityTypeBuilder<ParticipanteChamado> b)
    {
        b.ToTable("ParticipantesChamado"); b.HasKey(x => x.Id);
        b.HasOne<Chamado>().WithMany().HasForeignKey(x => x.ChamadoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tecnico>().WithMany().HasForeignKey(x => x.TecnicoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Papel).HasMaxLength(30).IsRequired();
        b.HasIndex(x => new { x.ChamadoId, x.TecnicoId }).IsUnique();
    }
}

public sealed class PosicaoTecnicoMap : IEntityTypeConfiguration<PosicaoTecnico>
{
    public void Configure(EntityTypeBuilder<PosicaoTecnico> b)
    {
        b.ToTable("PosicoesTecnico"); b.HasKey(x => x.Id);
        b.HasOne<Tecnico>().WithMany().HasForeignKey(x => x.TecnicoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Chamado>().WithMany().HasForeignKey(x => x.ChamadoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TecnicoId, x.DataHora });
    }
}

public sealed class NotificacaoPushMap : IEntityTypeConfiguration<NotificacaoPush>
{
    public void Configure(EntityTypeBuilder<NotificacaoPush> b)
    {
        b.ToTable("NotificacoesPush"); b.HasKey(x => x.Id);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioDestinatarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Chamado>().WithMany().HasForeignKey(x => x.ChamadoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        b.Property(x => x.Mensagem).HasMaxLength(2000).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ErroEnvio).HasMaxLength(2000);
        b.Property(x => x.ChaveDeduplicacao).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.ChaveDeduplicacao).IsUnique();
        b.HasIndex(x => new { x.Status, x.ProximaTentativaEm });
    }
}

public sealed class DispositivoPushMap : IEntityTypeConfiguration<DispositivoPush>
{
    public void Configure(EntityTypeBuilder<DispositivoPush> b)
    {
        b.ToTable("DispositivosPush"); b.HasKey(x => x.Id);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Endpoint).HasMaxLength(2000).IsRequired();
        b.Property(x => x.ChavePublica).HasMaxLength(500).IsRequired();
        b.Property(x => x.ChaveAutenticacao).HasMaxLength(500).IsRequired();
        b.HasIndex(x => x.Endpoint).IsUnique();
        b.HasIndex(x => x.UsuarioId);
    }
}

public sealed class AnexoChamadoMap : IEntityTypeConfiguration<AnexoChamado>
{
    public void Configure(EntityTypeBuilder<AnexoChamado> b)
    {
        b.ToTable("AnexosChamado"); b.HasKey(x => x.Id);
        b.HasOne<Chamado>().WithMany().HasForeignKey(x => x.ChamadoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.AutorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.NomeArquivo).HasMaxLength(255).IsRequired();
        b.Property(x => x.TipoConteudo).HasMaxLength(150).IsRequired();
        b.Property(x => x.Localizacao).HasMaxLength(2000).IsRequired();
        b.Property(x => x.ConteudoBase64).HasColumnType("TEXT");
        b.Property(x => x.TipoAnexo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => new { x.ChamadoId, x.TipoAnexo });
    }
}

public sealed class LancamentoPontoTecnicoMap : IEntityTypeConfiguration<LancamentoPontoTecnico>
{
    public void Configure(EntityTypeBuilder<LancamentoPontoTecnico> b)
    {
        b.ToTable("LancamentosPontosTecnico"); b.HasKey(x => x.Id);
        b.HasOne<Tecnico>().WithMany().HasForeignKey(x => x.TecnicoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Chamado>().WithMany().HasForeignKey(x => x.ChamadoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Situacao).HasMaxLength(20).IsRequired();
        b.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
        b.Property(x => x.ChaveIdempotencia).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.ChaveIdempotencia).IsUnique();
        b.HasIndex(x => new { x.TecnicoId, x.DataHora });
    }
}
