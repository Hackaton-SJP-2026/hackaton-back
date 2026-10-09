using FluentAssertions;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure;
using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Hackaton.Tests.Infrastructure;

public sealed class SqliteConfigurationTests
{
    [Fact]
    public async Task Configuration_UsesSqliteFileRelativeToProvidedBasePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "hackaton-sqlite-" + Guid.NewGuid());
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Data Source=data/test.db;Pooling=False"
        }).Build();
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped(_ => Mock.Of<IUserResolver>());
            services.AddInfrastructure(config, directory);
            await using var provider = services.BuildServiceProvider();
            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.Sqlite");
            var expected = Path.Combine(directory, "data", "test.db");
            context.Database.GetDbConnection().DataSource.Should().Be(expected);
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().NotBeNull();
            await context.Database.OpenConnectionAsync();
            File.Exists(expected).Should().BeTrue();
            await context.Database.CloseConnectionAsync();
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
