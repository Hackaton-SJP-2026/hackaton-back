using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Hackaton.Infrastructure.Data;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610090001_CriarCrudInicial")]
public sealed class CriarCrudInicial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Usuarios", table => new
        {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Nome = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
            Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
            Telefone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
            Secretaria = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
            Papel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
            Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
            CreationUserId = table.Column<Guid>(type: "TEXT", nullable: false),
            CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
            ModificationUserId = table.Column<Guid>(type: "TEXT", nullable: true),
            ModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Usuarios", x => x.Id));

        migrationBuilder.CreateTable("UnidadesAtendimento", table => new
        {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Nome = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
            Secretaria = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
            Departamento = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
            Endereco = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
            Latitude = table.Column<double>(type: "REAL", nullable: true),
            Longitude = table.Column<double>(type: "REAL", nullable: true),
            Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
            CreationUserId = table.Column<Guid>(type: "TEXT", nullable: false),
            CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
            ModificationUserId = table.Column<Guid>(type: "TEXT", nullable: true),
            ModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_UnidadesAtendimento", x => x.Id));

        migrationBuilder.CreateTable("Chamados", table => new
        {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Descricao = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
            SolicitanteId = table.Column<Guid>(type: "TEXT", nullable: false),
            UnidadeAtendimentoId = table.Column<Guid>(type: "TEXT", nullable: false),
            AreaAtendimento = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
            Natureza = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
            Modalidade = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
            Prioridade = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
            Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
            Equipamento = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
            Contato = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
            PessoasAfetadas = table.Column<int>(type: "INTEGER", nullable: false),
            ExisteAlternativa = table.Column<bool>(type: "INTEGER", nullable: false),
            NecessitaMaterial = table.Column<bool>(type: "INTEGER", nullable: false),
            LocalRetiradaMaterial = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
            TecnicoResponsavelId = table.Column<Guid>(type: "TEXT", nullable: true),
            InicioAtendimento = table.Column<DateTime>(type: "TEXT", nullable: true),
            CreationUserId = table.Column<Guid>(type: "TEXT", nullable: false),
            CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
            ModificationUserId = table.Column<Guid>(type: "TEXT", nullable: true),
            ModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Chamados", x => x.Id));

        migrationBuilder.CreateIndex("IX_Usuarios_Email", "Usuarios", "Email", unique: true);
        migrationBuilder.CreateIndex("IX_UnidadesAtendimento_Secretaria_Nome", "UnidadesAtendimento", new[] { "Secretaria", "Nome" });
        migrationBuilder.CreateIndex("IX_Chamados_SolicitanteId", "Chamados", "SolicitanteId");
        migrationBuilder.CreateIndex("IX_Chamados_UnidadeAtendimentoId", "Chamados", "UnidadeAtendimentoId");
        migrationBuilder.CreateIndex("IX_Chamados_Status", "Chamados", "Status");
        migrationBuilder.CreateIndex("IX_Chamados_TecnicoResponsavelId", "Chamados", "TecnicoResponsavelId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Chamados");
        migrationBuilder.DropTable("UnidadesAtendimento");
        migrationBuilder.DropTable("Usuarios");
    }
}
