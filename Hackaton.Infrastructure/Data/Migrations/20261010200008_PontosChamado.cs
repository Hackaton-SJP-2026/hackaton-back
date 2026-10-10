using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261010200008_PontosChamado")]
public sealed class PontosChamado : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "NivelComplexidade", table: "Chamados", type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Baixo");
        migrationBuilder.AddColumn<int>(name: "Pontos", table: "Chamados", type: "INTEGER", nullable: false, defaultValue: 5);
        migrationBuilder.Sql("UPDATE Chamados SET Pontos = CASE WHEN Prioridade = 'Alta' THEN 10 ELSE 5 END;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Pontos", table: "Chamados");
        migrationBuilder.DropColumn(name: "NivelComplexidade", table: "Chamados");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.2");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
