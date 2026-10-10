using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hackaton.Infrastructure.Data.Migrations.MariaDb
{
    /// <inheritdoc />
    public partial class AtualizacoesEquipeMariaDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Responsavel",
                table: "TecnicoUnidadeAtendimento",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Patente",
                table: "Tecnicos",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Pontos",
                table: "Tecnicos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NivelComplexidade",
                table: "Chamados",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Pontos",
                table: "Chamados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConteudoBase64",
                table: "AnexosChamado",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "UnidadeResponsavelId",
                table: "TecnicoUnidadeAtendimento",
                type: "char(36)",
                nullable: true,
                computedColumnSql: "CASE WHEN Responsavel = 1 THEN RTRIM(UnidadeAtendimentoId) ELSE NULL END",
                stored: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_TecnicoUnidadeAtendimento_UnidadeResponsavelId",
                table: "TecnicoUnidadeAtendimento",
                column: "UnidadeResponsavelId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TecnicoUnidadeAtendimento_UnidadeResponsavelId",
                table: "TecnicoUnidadeAtendimento");

            migrationBuilder.DropColumn(
                name: "UnidadeResponsavelId",
                table: "TecnicoUnidadeAtendimento");

            migrationBuilder.DropColumn(
                name: "Responsavel",
                table: "TecnicoUnidadeAtendimento");

            migrationBuilder.DropColumn(
                name: "Patente",
                table: "Tecnicos");

            migrationBuilder.DropColumn(
                name: "Pontos",
                table: "Tecnicos");

            migrationBuilder.DropColumn(
                name: "NivelComplexidade",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "Pontos",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "ConteudoBase64",
                table: "AnexosChamado");
        }
    }
}
