using System.Text.Json;
using MySqlConnector;
using Hackaton.Infrastructure.Data;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Enums;
using Microsoft.EntityFrameworkCore;

var configPath = args.Length > 0 ? args[0] : "HackatonApi/appsettings.json";
using var json = JsonDocument.Parse(File.ReadAllText(configPath));
var connectionString = json.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString();
await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();
await using var command = connection.CreateCommand();
command.CommandText = "SELECT VERSION()";
Console.WriteLine("Servidor: " + await command.ExecuteScalarAsync());
command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE()";
Console.WriteLine("Tabelas: " + await command.ExecuteScalarAsync());
var options = new DbContextOptionsBuilder<MariaDbAppDbContext>()
    .UseMySql(connectionString, new MariaDbServerVersion(Version.Parse(json.RootElement.GetProperty("Persistence").GetProperty("ServerVersion").GetString()!))).Options;
await using var db = new MariaDbAppDbContext(options);
Console.WriteLine("Migrations aplicadas: " + string.Join(", ", await db.Database.GetAppliedMigrationsAsync()));
if (!(await db.Database.GetAppliedMigrationsAsync()).Any()) return;
Console.WriteLine("Migrations pendentes: " + (await db.Database.GetPendingMigrationsAsync()).Count());
await using var transaction = await db.Database.BeginTransactionAsync();
try
{
    var prefeitura = new Prefeitura { Nome = "Verificação temporária", Cidade = "São José dos Pinhais", Uf = "PR" };
    var user = new Usuario { Nome = "Usuário temporário", Email = Guid.NewGuid()+"@example.test", PrefeituraId = prefeitura.Id };
    var unidade = new UnidadeAtendimento { Nome = "Unidade temporária", Endereco = "Endereço de teste", PrefeituraId = prefeitura.Id };
    var tecnico = new Tecnico { UsuarioId = user.Id };
    var chamado = new Chamado { SolicitanteId = user.Id, UnidadeAtendimentoId = unidade.Id, TecnicoResponsavelId = tecnico.Id,
        Descricao = "Verificação de acentuação: análise ✅", AreaAtendimento = "TI", Natureza = "Software", Modalidade = "Remoto",
        Status = StatusChamado.EmAnalise, EtapaAtendimento = "Vinculado" };
    db.AddRange(prefeitura, user, unidade, tecnico, chamado);
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear();
    var loaded = await db.Chamados.SingleAsync(x => x.Id == chamado.Id);
    if (loaded.Descricao != chamado.Descricao || loaded.Status != StatusChamado.EmAnalise || loaded.EtapaAtendimento != "Vinculado")
        throw new InvalidOperationException("Roundtrip de chamado divergente.");
    var found = await db.Usuarios.SingleAsync(x => x.PrefeituraId == prefeitura.Id && x.Email.ToLower() == user.Email.ToLower() && x.Ativo);
    if (found.Id != user.Id) throw new InvalidOperationException("Consulta do login divergente.");
    Console.WriteLine("EF: escrita/leitura, GUIDs, vínculos, consulta do login, status e Unicode aprovados.");
}
finally
{
    await transaction.RollbackAsync();
    Console.WriteLine("Transação de verificação revertida; dados temporários não permaneceram no banco.");
}
