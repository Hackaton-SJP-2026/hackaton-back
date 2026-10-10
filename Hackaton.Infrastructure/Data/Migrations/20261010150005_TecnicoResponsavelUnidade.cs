using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261010150005_TecnicoResponsavelUnidade")]
public sealed class TecnicoResponsavelUnidade : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "Responsavel",
            table: "TecnicoUnidadeAtendimento",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_TecnicoUnidadeAtendimento_UnidadeAtendimentoId_Responsavel",
            table: "TecnicoUnidadeAtendimento",
            column: "UnidadeAtendimentoId",
            unique: true,
            filter: "\"Responsavel\" = 1");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TecnicoUnidadeAtendimento_UnidadeAtendimentoId_Responsavel",
            table: "TecnicoUnidadeAtendimento");

        migrationBuilder.DropColumn(name: "Responsavel", table: "TecnicoUnidadeAtendimento");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.2");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
