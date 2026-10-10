using FluentAssertions;
using Hackaton.Infrastructure;
using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hackaton.Tests.Infrastructure;

public sealed class MariaDbConfigurationTests
{
    [Fact]
    public void Configuration_UsesSameScopedContextAndMariaDbOnlyMigrationsWithoutNetwork()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Persistence:Provider"] = "MariaDb",
            ["Persistence:ServerVersion"] = "11.8.9",
            ["ConnectionStrings:DefaultConnection"] = "Server=unreachable.invalid;Database=test;User ID=test;Password=test;"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Should().BeSameAs(scope.ServiceProvider.GetRequiredService<MariaDbAppDbContext>());
        context.Database.ProviderName.Should().Be("Pomelo.EntityFrameworkCore.MySql");
        context.Database.GetMigrations().Should().HaveCount(2);
        context.Database.GetMigrations().Should().OnlyContain(x => x.EndsWith("_InicialMariaDb") || x.EndsWith("_AtualizacoesEquipeMariaDb"));
        var script = context.GetService<IMigrator>().GenerateScript();
        script.Should().Contain("utf8mb4").And.Contain("char(36)").And.Contain("datetime(6)");
        script.Should().Contain("ascii_bin").And.NotContain("CREATE TEMP TABLE");
        script.Should().Contain("EtapaAtendimento").And.Contain("PasswordHash");
        script.Should().Contain("UnidadeResponsavelId").And.Contain("longtext");
        using var otherScope = provider.CreateScope();
        context.Should().NotBeSameAs(otherScope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    [Fact]
    public void Configuration_RejectsUnknownProvider()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Persistence:Provider"] = "Unknown",
            ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:"
        }).Build();
        var configure = () => new ServiceCollection().AddInfrastructure(configuration);
        configure.Should().Throw<InvalidOperationException>().WithMessage("*MariaDb ou Sqlite*");
    }
}
