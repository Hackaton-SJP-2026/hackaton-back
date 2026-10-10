using System.Text.Json;
using FluentAssertions;
using Hackaton.Domain.Enums;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using HackatonApi.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Hackaton.Tests.Infrastructure;

public sealed class ChamadoStatusTests
{
    [Theory]
    [InlineData(StatusChamado.Aberto, "Aberto")]
    [InlineData(StatusChamado.EmAnalise, "Em Análise")]
    [InlineData(StatusChamado.Resolvido, "Resolvido")]
    public void Json_UsesOnlyPublicLabels(StatusChamado status, string label)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new StatusChamadoJsonConverter());
        var json = JsonSerializer.Serialize(status, options);
        JsonSerializer.Deserialize<string>(json).Should().Be(label);
        JsonSerializer.Deserialize<StatusChamado>(json, options).Should().Be(status);
        foreach (var invalid in new[] { "1", "99", "\"Vinculado\"", "\"EmAnalise\"", "\"Em análise\"" })
        {
            Action read = () => JsonSerializer.Deserialize<StatusChamado>(invalid, options);
            read.Should().Throw<JsonException>();
        }
    }

    [Theory]
    [InlineData("Aberto", StatusChamado.Aberto)]
    [InlineData("Vinculado", StatusChamado.EmAnalise)]
    [InlineData("Em andamento", StatusChamado.EmAnalise)]
    [InlineData("Pausado", StatusChamado.EmAnalise)]
    [InlineData("Bloqueado", StatusChamado.EmAnalise)]
    [InlineData("Aguardando validação", StatusChamado.EmAnalise)]
    [InlineData("Aguardando análise", StatusChamado.EmAnalise)]
    [InlineData("Resolvido", StatusChamado.Resolvido)]
    public async Task Migration_PreservesLegacyStageAndHistory(string legacy, StatusChamado expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        var migrator = db.GetService<IMigrator>();
        await PrepareLegacyDatabase(db);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Chamados SET Status = {legacy}");
        var eventsBefore = await db.EventosChamado.CountAsync();
        await migrator.MigrateAsync();
        var tickets = await db.Chamados.AsNoTracking().ToListAsync();
        tickets.Should().NotBeEmpty();
        tickets.Should().OnlyContain(t => t.Status == expected && t.EtapaAtendimento == legacy);
        (await db.EventosChamado.CountAsync()).Should().Be(eventsBefore);
        (await db.Chamados.CountAsync(t => t.Status == expected)).Should().Be(tickets.Count);
        await migrator.MigrateAsync();
        (await db.Chamados.CountAsync()).Should().Be(tickets.Count);
    }

    private static async Task PrepareLegacyDatabase(AppDbContext db)
    {
        // Reproduz o schema anterior sem depender dos seeds históricos sem TargetModel.
        await db.Database.EnsureCreatedAsync();
        var prefeitura = new Prefeitura { Nome = "Teste", Cidade = "Teste", Uf = "PR" };
        var usuario = new Usuario { PrefeituraId = prefeitura.Id, Nome = "Teste", Email = "teste@example.test" };
        var unidade = new UnidadeAtendimento { PrefeituraId = prefeitura.Id, Nome = "Teste", Endereco = "Teste" };
        var chamado = new Chamado { SolicitanteId = usuario.Id, UnidadeAtendimentoId = unidade.Id, Descricao = "Teste", AreaAtendimento = "TI", Natureza = "Software", Modalidade = "Remoto" };
        db.AddRange(prefeitura, usuario, unidade, chamado,
            new EventoChamado { ChamadoId = chamado.Id, Tipo = "Abertura", Descricao = "Histórico preservado" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Chamados DROP COLUMN EtapaAtendimento");
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE __EFMigrationsHistory (MigrationId TEXT NOT NULL PRIMARY KEY, ProductVersion TEXT NOT NULL)");
        foreach (var id in db.Database.GetMigrations().Where(id => id != "20261010130004_SepararStatusEtapaChamado"))
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({id}, {"8.0.2"})");
    }

    [Fact]
    public async Task Migration_RejectsUnknownStatusWithoutOverwritingData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        var migrator = db.GetService<IMigrator>();
        await PrepareLegacyDatabase(db);
        await db.Database.ExecuteSqlRawAsync("UPDATE Chamados SET Status = 'Desconhecido'");
        Func<Task> migrate = () => migrator.MigrateAsync();
        await migrate.Should().ThrowAsync<SqliteException>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Chamados WHERE Status = 'Desconhecido'";
        Convert.ToInt32(await command.ExecuteScalarAsync()).Should().BeGreaterThan(0);
    }
}
