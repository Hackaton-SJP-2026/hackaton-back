using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hackaton.Application.Exceptions;
using Hackaton.Application.Interface;
using Hackaton.Tests.Fixtures;
using HackatonApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Hackaton.Tests.Api;

public sealed class ApiTests
{
    private static WebApplicationFactory<Program> Factory(string environment = "Development")
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Enabled"] = "false",
                ["Authentication:Enabled"] = "false"
            }));
            builder.ConfigureServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(TestController).Assembly);
                services.AddScoped(_ => Mock.Of<IAppService<TestEntity, TestDto>>());
            });
        });

    [Fact]
    public async Task StatusAndSwagger_WorkWithoutDatabase()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        (await client.GetAsync("/api/status")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/test")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedCrud_RejectsUnauthenticatedWrites()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        (await client.PostAsJsonAsync("/api/test", new TestDto { Text = "example" }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("validation", HttpStatusCode.BadRequest)]
    [InlineData("missing", HttpStatusCode.NotFound)]
    public async Task Exceptions_AreTranslatedToHttp(string type, HttpStatusCode expected)
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        (await client.GetAsync("/api/test/errors/" + type)).StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task Production_DoesNotExposeUnexpectedExceptionDetails()
    {
        using var factory = Factory("Production");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.GetAsync("/api/test/errors/unexpected");
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("test-secret-detail");
        (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

// Controller apenas do assembly de testes, não exposto pela API distribuída.
public sealed class TestController(IAppService<TestEntity, TestDto> service) : BaseController<TestEntity, TestDto>(service)
{
    [HttpGet("errors/{type}")]
    [AllowAnonymous]
    public IActionResult Error(string type) => throw type switch
    {
        "validation" => new EntityValidationException("invalid"),
        "missing" => new EntityNotFoundException("test", Guid.Empty),
        _ => new InvalidOperationException("test-secret-detail")
    };
}
