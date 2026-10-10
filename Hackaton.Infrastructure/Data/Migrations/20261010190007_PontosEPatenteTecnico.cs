using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261010190007_PontosEPatenteTecnico")]
public sealed class PontosEPatenteTecnico : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Pontos", table: "Tecnicos", type: "INTEGER", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<string>(
            name: "Patente", table: "Tecnicos", type: "TEXT", maxLength: 30, nullable: false, defaultValue: "Soldado raso");
        migrationBuilder.Sql("UPDATE Tecnicos SET Pontos = COALESCE((SELECT SUM(Pontos) FROM LancamentosPontosTecnico p WHERE p.TecnicoId = Tecnicos.Id), 0);");
        migrationBuilder.Sql("UPDATE Tecnicos SET Patente = CASE WHEN Pontos >= 5000 THEN 'General' WHEN Pontos >= 3600 THEN 'Coronel' WHEN Pontos >= 2600 THEN 'Major' WHEN Pontos >= 1800 THEN 'Capitão' WHEN Pontos >= 1200 THEN 'Tenente' WHEN Pontos >= 800 THEN 'Subtenente' WHEN Pontos >= 500 THEN 'Sargento' WHEN Pontos >= 250 THEN 'Cabo' WHEN Pontos >= 100 THEN 'Soldado' ELSE 'Soldado raso' END;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Pontos", table: "Tecnicos");
        migrationBuilder.DropColumn(name: "Patente", table: "Tecnicos");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.2");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
