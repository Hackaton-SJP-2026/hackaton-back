using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610100003_AuthenticationAndTechnicianPoints")]
public sealed class AuthenticationAndTechnicianPoints : Migration
{
    private static readonly Guid TecnicoId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
    private static readonly Guid ChamadoId = Guid.Parse("22222222-0000-0000-0000-000000000002");
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly DateTime SeedTime = new(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc);
    private const string DemoPasswordHash = "AQAAAAIAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v+5SEiHVKPG896UDupZ+ytYAHJH2WRStz6ySyrLzrj7AQ==";

    protected override void Up(MigrationBuilder m)
    {
        m.AddColumn<string>("PasswordHash", "Usuarios", type: "TEXT", maxLength: 512, nullable: true);
        m.CreateTable(name: "LancamentosPontosTecnico", columns: t => new
        {
            Id = t.Column<Guid>("TEXT", nullable: false), TecnicoId = t.Column<Guid>("TEXT", nullable: false),
            ChamadoId = t.Column<Guid>("TEXT", nullable: true), Pontos = t.Column<int>("INTEGER", nullable: false),
            Situacao = t.Column<string>("TEXT", maxLength: 20, nullable: false), Motivo = t.Column<string>("TEXT", maxLength: 500, nullable: false),
            ChaveIdempotencia = t.Column<string>("TEXT", maxLength: 300, nullable: false), DataHora = t.Column<DateTime>("TEXT", nullable: false),
            CreationUserId = t.Column<Guid>("TEXT", nullable: false), CreationTime = t.Column<DateTime>("TEXT", nullable: false),
            ModificationUserId = t.Column<Guid>("TEXT", nullable: true), ModificationTime = t.Column<DateTime>("TEXT", nullable: true)
        }, constraints: c =>
        {
            c.PrimaryKey("PK_LancamentosPontosTecnico", x => x.Id);
            c.ForeignKey("FK_LancamentosPontosTecnico_Tecnicos", x => x.TecnicoId, "Tecnicos", "Id", onDelete: ReferentialAction.Restrict);
            c.ForeignKey("FK_LancamentosPontosTecnico_Chamados", x => x.ChamadoId, "Chamados", "Id", onDelete: ReferentialAction.Restrict);
        });
        m.CreateIndex("IX_LancamentosPontosTecnico_ChaveIdempotencia", "LancamentosPontosTecnico", "ChaveIdempotencia", unique: true);
        m.CreateIndex("IX_LancamentosPontosTecnico_TecnicoId_DataHora", "LancamentosPontosTecnico", new[] { "TecnicoId", "DataHora" });

        foreach (var id in new[]
        {
            Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
            Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"),
            Guid.Parse("cccccccc-0000-0000-0000-000000000001")
        })
            m.Sql($"UPDATE Usuarios SET PasswordHash = '{DemoPasswordHash}' WHERE Id = '{id}'");

        m.InsertData("LancamentosPontosTecnico", new[] { "Id", "TecnicoId", "ChamadoId", "Pontos", "Situacao", "Motivo", "ChaveIdempotencia", "DataHora", "CreationUserId", "CreationTime" }, new object[]
        {
            Guid.Parse("abababab-0000-0000-0000-000000000001"), TecnicoId, ChamadoId, 10, "Provisorio",
            "Pontos demonstrativos por chamado aguardando validação.", "seed:chamado-validacao:tecnico", SeedTime.AddHours(-1), AdminId, SeedTime.AddHours(-1)
        });
    }

    protected override void Down(MigrationBuilder m)
    {
        m.DeleteData("LancamentosPontosTecnico", "Id", Guid.Parse("abababab-0000-0000-0000-000000000001"));
        foreach (var id in new[]
        {
            Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
            Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"),
            Guid.Parse("cccccccc-0000-0000-0000-000000000001")
        })
            m.Sql($"UPDATE Usuarios SET PasswordHash = NULL WHERE Id = '{id}'");
        m.DropTable("LancamentosPontosTecnico");
        m.DropColumn("PasswordHash", "Usuarios");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.2");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
