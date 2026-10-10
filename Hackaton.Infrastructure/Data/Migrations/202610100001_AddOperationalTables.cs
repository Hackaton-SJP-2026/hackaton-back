using Hackaton.Infrastructure.Data;
using Hackaton.Infrastructure.Data.Map;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610100001_AddOperationalTables")]
public sealed class AddOperationalTables : Migration
{
    private static readonly Guid LegacyPrefeituraId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable(name: "Prefeituras", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), Nome = t.Column<string>("TEXT", maxLength: 180, nullable: false),
            Cnpj = t.Column<string>("TEXT", maxLength: 14, nullable: true), Cidade = t.Column<string>("TEXT", maxLength: 120, nullable: false),
            Uf = t.Column<string>("TEXT", maxLength: 2, nullable: false), Ativa = t.Column<bool>("INTEGER", nullable: false),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false),
            ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => c.PrimaryKey("PK_Prefeituras", x => x.Id));
        m.InsertData("Prefeituras", new[] { "Id", "Nome", "Cidade", "Uf", "Ativa", "CreationUserId", "CreationTime" }, new object[]
        { LegacyPrefeituraId, "Prefeitura migrada - revisar cadastro", "Não informado", "NA", true, Guid.Empty, DateTime.UtcNow });

        m.AddColumn<Guid>("PrefeituraId", "Usuarios", type: "TEXT", nullable: false, defaultValue: LegacyPrefeituraId);
        m.AddColumn<Guid>("PrefeituraId", "UnidadesAtendimento", type: "TEXT", nullable: false, defaultValue: LegacyPrefeituraId);
        m.AddForeignKey("FK_Usuarios_Prefeituras_PrefeituraId", "Usuarios", "PrefeituraId", "Prefeituras", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        m.AddForeignKey("FK_UnidadesAtendimento_Prefeituras_PrefeituraId", "UnidadesAtendimento", "PrefeituraId", "Prefeituras", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        m.DropIndex("IX_Usuarios_Email", "Usuarios");
        m.DropColumn("Secretaria", "Usuarios");
        m.CreateIndex("IX_Usuarios_PrefeituraId_Email", "Usuarios", new[] { "PrefeituraId", "Email" }, unique: true);
        m.DropIndex("IX_UnidadesAtendimento_Secretaria_Nome", "UnidadesAtendimento");
        m.DropColumn("Secretaria", "UnidadesAtendimento");
        m.DropColumn("Departamento", "UnidadesAtendimento");
        m.CreateIndex("IX_UnidadesAtendimento_PrefeituraId_Nome", "UnidadesAtendimento", new[] { "PrefeituraId", "Nome" });

        m.CreateTable(name: "Tecnicos", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), UsuarioId = t.Column<Guid>("TEXT", nullable: false),
            FotoUrl = t.Column<string>("TEXT", maxLength: 1000, nullable: true), Disponibilidade = t.Column<string>("TEXT", maxLength: 30, nullable: false),
            MotivoIndisponibilidade = t.Column<string>("TEXT", maxLength: 500, nullable: true), DisponibilidadeAtualizadaEm = t.Column<DateTime>("TEXT", nullable: true),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false),
            ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => { c.PrimaryKey("PK_Tecnicos", x => x.Id); c.ForeignKey("FK_Tecnicos_Usuarios_UsuarioId", x => x.UsuarioId, "Usuarios", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_Tecnicos_UsuarioId", "Tecnicos", "UsuarioId", unique: true);

        m.CreateTable(name: "TecnicoUnidadeAtendimento", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), TecnicoId = t.Column<Guid>("TEXT", nullable: false),
            UnidadeAtendimentoId = t.Column<Guid>("TEXT", nullable: false), Ativo = t.Column<bool>("INTEGER", nullable: false),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false),
            ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => { c.PrimaryKey("PK_TecnicoUnidadeAtendimento", x => x.Id); c.ForeignKey("FK_TecnicoUnidadeAtendimento_Tecnicos_TecnicoId", x => x.TecnicoId, "Tecnicos", "Id", onDelete: ReferentialAction.Restrict); c.ForeignKey("FK_TecnicoUnidadeAtendimento_UnidadesAtendimento_UnidadeAtendimentoId", x => x.UnidadeAtendimentoId, "UnidadesAtendimento", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_TecnicoUnidadeAtendimento_TecnicoId_UnidadeAtendimentoId", "TecnicoUnidadeAtendimento", new[] { "TecnicoId", "UnidadeAtendimentoId" }, unique: true);

        m.AddForeignKey("FK_Chamados_Solicitante", "Chamados", "SolicitanteId", "Usuarios", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        m.AddForeignKey("FK_Chamados_Unidade", "Chamados", "UnidadeAtendimentoId", "UnidadesAtendimento", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        m.AddForeignKey("FK_Chamados_Tecnico", "Chamados", "TecnicoResponsavelId", "Tecnicos", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

        m.CreateTable(name: "EventosChamado", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), ChamadoId = t.Column<Guid>("TEXT", nullable: false), AutorUsuarioId = t.Column<Guid>("TEXT", nullable: true),
            Tipo = t.Column<string>("TEXT", maxLength: 40, nullable: false), Descricao = t.Column<string>("TEXT", maxLength: 4000, nullable: false), DataHora = t.Column<DateTime>("TEXT", nullable: false),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false), ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => { c.PrimaryKey("PK_EventosChamado", x => x.Id); c.ForeignKey("FK_EventosChamado_Chamados", x => x.ChamadoId, "Chamados", "Id", onDelete: ReferentialAction.Restrict); c.ForeignKey("FK_EventosChamado_Usuarios", x => x.AutorUsuarioId, "Usuarios", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_EventosChamado_ChamadoId_DataHora", "EventosChamado", new[] { "ChamadoId", "DataHora" });

        m.CreateTable(name: "ComentariosChamado", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), ChamadoId = t.Column<Guid>("TEXT", nullable: false), AutorUsuarioId = t.Column<Guid>("TEXT", nullable: false),
            Texto = t.Column<string>("TEXT", maxLength: 4000, nullable: false), DataHora = t.Column<DateTime>("TEXT", nullable: false),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false), ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => { c.PrimaryKey("PK_ComentariosChamado", x => x.Id); c.ForeignKey("FK_ComentariosChamado_Chamados", x => x.ChamadoId, "Chamados", "Id", onDelete: ReferentialAction.Restrict); c.ForeignKey("FK_ComentariosChamado_Usuarios", x => x.AutorUsuarioId, "Usuarios", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_ComentariosChamado_ChamadoId_DataHora", "ComentariosChamado", new[] { "ChamadoId", "DataHora" });

        m.CreateTable(name: "ParticipantesChamado", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), ChamadoId = t.Column<Guid>("TEXT", nullable: false), TecnicoId = t.Column<Guid>("TEXT", nullable: false),
            Papel = t.Column<string>("TEXT", maxLength: 30, nullable: false), DataVinculo = t.Column<DateTime>("TEXT", nullable: false), DataDesvinculo = t.Column<DateTime>("TEXT", nullable: true),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false), ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => { c.PrimaryKey("PK_ParticipantesChamado", x => x.Id); c.ForeignKey("FK_ParticipantesChamado_Chamados", x => x.ChamadoId, "Chamados", "Id", onDelete: ReferentialAction.Restrict); c.ForeignKey("FK_ParticipantesChamado_Tecnicos", x => x.TecnicoId, "Tecnicos", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_ParticipantesChamado_ChamadoId_TecnicoId", "ParticipantesChamado", new[] { "ChamadoId", "TecnicoId" }, unique: true);

        m.CreateTable(name: "PosicoesTecnico", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), TecnicoId = t.Column<Guid>("TEXT", nullable: false), ChamadoId = t.Column<Guid>("TEXT", nullable: true),
            Latitude = t.Column<double>("REAL", nullable: false), Longitude = t.Column<double>("REAL", nullable: false), PrecisaoMetros = t.Column<double>("REAL", nullable: true), DataHora = t.Column<DateTime>("TEXT", nullable: false)
        }, constraints: c => { c.PrimaryKey("PK_PosicoesTecnico", x => x.Id); c.ForeignKey("FK_PosicoesTecnico_Tecnicos", x => x.TecnicoId, "Tecnicos", "Id", onDelete: ReferentialAction.Restrict); c.ForeignKey("FK_PosicoesTecnico_Chamados", x => x.ChamadoId, "Chamados", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_PosicoesTecnico_TecnicoId_DataHora", "PosicoesTecnico", new[] { "TecnicoId", "DataHora" });

        m.CreateTable(name: "NotificacoesPush", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), UsuarioDestinatarioId = t.Column<Guid>("TEXT", nullable: false), ChamadoId = t.Column<Guid>("TEXT", nullable: true),
            Titulo = t.Column<string>("TEXT", maxLength: 200, nullable: false), Mensagem = t.Column<string>("TEXT", maxLength: 2000, nullable: false), Status = t.Column<string>("TEXT", maxLength: 30, nullable: false),
            Tentativas = t.Column<int>("INTEGER", nullable: false), CriadaEm = t.Column<DateTime>("TEXT", nullable: false), EnviadaEm = t.Column<DateTime>("TEXT", nullable: true), ProximaTentativaEm = t.Column<DateTime>("TEXT", nullable: true),
            ErroEnvio = t.Column<string>("TEXT", maxLength: 2000, nullable: true), ChaveDeduplicacao = t.Column<string>("TEXT", maxLength: 300, nullable: false)
        }, constraints: c => { c.PrimaryKey("PK_NotificacoesPush", x => x.Id); c.ForeignKey("FK_NotificacoesPush_Usuarios", x => x.UsuarioDestinatarioId, "Usuarios", "Id", onDelete: ReferentialAction.Restrict); c.ForeignKey("FK_NotificacoesPush_Chamados", x => x.ChamadoId, "Chamados", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_NotificacoesPush_ChaveDeduplicacao", "NotificacoesPush", "ChaveDeduplicacao", unique: true);
        m.CreateIndex("IX_NotificacoesPush_Status_ProximaTentativaEm", "NotificacoesPush", new[] { "Status", "ProximaTentativaEm" });

        m.CreateTable(name: "DispositivosPush", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), UsuarioId = t.Column<Guid>("TEXT", nullable: false), Endpoint = t.Column<string>("TEXT", maxLength: 2000, nullable: false),
            ChavePublica = t.Column<string>("TEXT", maxLength: 500, nullable: false), ChaveAutenticacao = t.Column<string>("TEXT", maxLength: 500, nullable: false), Ativo = t.Column<bool>("INTEGER", nullable: false), UltimoUsoEm = t.Column<DateTime>("TEXT", nullable: true),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false), ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => { c.PrimaryKey("PK_DispositivosPush", x => x.Id); c.ForeignKey("FK_DispositivosPush_Usuarios", x => x.UsuarioId, "Usuarios", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_DispositivosPush_Endpoint", "DispositivosPush", "Endpoint", unique: true);
        m.CreateIndex("IX_DispositivosPush_UsuarioId", "DispositivosPush", "UsuarioId");

        m.CreateTable(name: "AnexosChamado", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), ChamadoId = t.Column<Guid>("TEXT", nullable: false), AutorUsuarioId = t.Column<Guid>("TEXT", nullable: false),
            NomeArquivo = t.Column<string>("TEXT", maxLength: 255, nullable: false), TipoConteudo = t.Column<string>("TEXT", maxLength: 150, nullable: false), TamanhoBytes = t.Column<long>("INTEGER", nullable: false),
            Localizacao = t.Column<string>("TEXT", maxLength: 2000, nullable: false), TipoAnexo = t.Column<string>("TEXT", maxLength: 30, nullable: false), DataHora = t.Column<DateTime>("TEXT", nullable: false),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false), ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c => { c.PrimaryKey("PK_AnexosChamado", x => x.Id); c.ForeignKey("FK_AnexosChamado_Chamados", x => x.ChamadoId, "Chamados", "Id", onDelete: ReferentialAction.Restrict); c.ForeignKey("FK_AnexosChamado_Usuarios", x => x.AutorUsuarioId, "Usuarios", "Id", onDelete: ReferentialAction.Restrict); });
        m.CreateIndex("IX_AnexosChamado_ChamadoId_TipoAnexo", "AnexosChamado", new[] { "ChamadoId", "TipoAnexo" });
    }

    protected override void Down(MigrationBuilder m)
    {
        m.DropTable("AnexosChamado"); m.DropTable("DispositivosPush"); m.DropTable("NotificacoesPush"); m.DropTable("PosicoesTecnico");
        m.DropTable("ParticipantesChamado"); m.DropTable("ComentariosChamado"); m.DropTable("EventosChamado");
        m.DropTable("TecnicoUnidadeAtendimento"); m.DropTable("Tecnicos");
        m.DropForeignKey("FK_Chamados_Tecnico", "Chamados"); m.DropForeignKey("FK_Chamados_Unidade", "Chamados"); m.DropForeignKey("FK_Chamados_Solicitante", "Chamados");
        m.DropForeignKey("FK_UnidadesAtendimento_Prefeituras_PrefeituraId", "UnidadesAtendimento"); m.DropForeignKey("FK_Usuarios_Prefeituras_PrefeituraId", "Usuarios");
        m.DropIndex("IX_Usuarios_PrefeituraId_Email", "Usuarios");
        m.DropIndex("IX_UnidadesAtendimento_PrefeituraId_Nome", "UnidadesAtendimento");
        m.AddColumn<string>("Secretaria", "Usuarios", type: "TEXT", maxLength: 120, nullable: false, defaultValue: "");
        m.AddColumn<string>("Secretaria", "UnidadesAtendimento", type: "TEXT", maxLength: 120, nullable: false, defaultValue: "");
        m.AddColumn<string>("Departamento", "UnidadesAtendimento", type: "TEXT", maxLength: 120, nullable: false, defaultValue: "");
        m.CreateIndex("IX_Usuarios_Email", "Usuarios", "Email", unique: true);
        m.CreateIndex("IX_UnidadesAtendimento_Secretaria_Nome", "UnidadesAtendimento", new[] { "Secretaria", "Nome" });
        m.DropColumn("PrefeituraId", "UnidadesAtendimento"); m.DropColumn("PrefeituraId", "Usuarios"); m.DropTable("Prefeituras");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.2");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly,
            type => type != typeof(LancamentoPontoTecnicoMap));
        modelBuilder.Entity<Hackaton.Domain.Entities.Usuario>().Ignore(x => x.PasswordHash);
    }
}
