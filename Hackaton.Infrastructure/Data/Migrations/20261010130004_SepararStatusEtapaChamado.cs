using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261010130004_SepararStatusEtapaChamado")]
public sealed class SepararStatusEtapaChamado : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        // Falha antes de converter se houver um status desconhecido; não descarta sua semântica.
        m.Sql("CREATE TEMP TABLE ValidacaoStatusChamado (Valor INTEGER CHECK (Valor = 0));");
        m.Sql("INSERT INTO ValidacaoStatusChamado SELECT COUNT(*) FROM Chamados WHERE Status NOT IN ('Aberto', 'Em aberto', 'Em Análise', 'Em análise', 'EmAnalise', 'Vinculado', 'Em andamento', 'Pausado', 'Bloqueado', 'Aguardando validação', 'Aguardando análise', 'Resolvido');");
        m.Sql("DROP TABLE ValidacaoStatusChamado;");
        m.AddColumn<string>("EtapaAtendimento", "Chamados", type: "TEXT", maxLength: 40, nullable: false, defaultValue: "Aberto");
        m.Sql("UPDATE Chamados SET EtapaAtendimento = CASE WHEN Status = 'Em aberto' THEN 'Aberto' WHEN Status IN ('Em Análise', 'Em análise', 'EmAnalise') THEN CASE WHEN TecnicoResponsavelId IS NULL THEN 'Aberto' ELSE 'Vinculado' END ELSE Status END;");
        m.Sql("UPDATE Chamados SET Status = CASE WHEN Status IN ('Aberto', 'Em aberto') THEN 'Aberto' WHEN Status = 'Resolvido' THEN 'Resolvido' ELSE 'EmAnalise' END;");
    }
    protected override void Down(MigrationBuilder m)
    {
        m.Sql("UPDATE Chamados SET Status = CASE WHEN Status = 'EmAnalise' THEN EtapaAtendimento ELSE Status END;");
        m.DropColumn("EtapaAtendimento", "Chamados");
    }
}
