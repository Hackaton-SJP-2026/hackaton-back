using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using HackatonApi.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hackaton.Tests.Api;

public sealed class UserLoginTests
{
    [Fact]
    public async Task WebLogin_AuthenticatesUserAndProtectsPortalByTenant()
    {
        var database = Path.Combine(Path.GetTempPath(), "hackaton-login-" + Guid.NewGuid() + ".db");
        var signingKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        try
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                foreach (var setting in new Dictionary<string, string>
                {
                    ["Persistence:Enabled"] = "true",
                    ["Persistence:Provider"] = "Sqlite",
                    ["Authentication:Enabled"] = "true",
                    ["ConnectionStrings:DefaultConnection"] = "Data Source=" + database + ";Pooling=False",
                    ["JwtSettings:Issuer"] = "login-test",
                    ["JwtSettings:Audience"] = "login-test-client",
                    ["JwtSettings:SigningKey"] = signingKey
                }) builder.UseSetting(setting.Key, setting.Value);
            });
            using var client = factory.CreateClient();
            var prefeitura = new Prefeitura { Nome = "Prefeitura de teste", Cidade = "Teste", Uf = "PR" };
            var outra = new Prefeitura { Nome = "Outra prefeitura", Cidade = "Outra", Uf = "PR" };
            const string password = "Teste@12345!";
            Usuario usuario;
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync();
                usuario = new Usuario { Nome = "Solicitante", Email = "solicitante@example.test", PrefeituraId = prefeitura.Id, Papel = "Usuario" };
                var tecnico = new Usuario { Nome = "Técnico", Email = "tecnico@example.test", PrefeituraId = prefeitura.Id, Papel = "Tecnico" };
                var inactive = new Usuario { Nome = "Inativo", Email = "inativo@example.test", PrefeituraId = prefeitura.Id, Papel = "Usuario", Ativo = false };
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
                foreach (var account in new[] { usuario, tecnico, inactive }) account.PasswordHash = hasher.HashPassword(account, password);
                var unidade = new UnidadeAtendimento { Nome = "Unidade", Endereco = "Endereço", PrefeituraId = outra.Id };
                var otherUser = new Usuario { Nome = "Outro solicitante", Email = "outro@example.test", PrefeituraId = outra.Id };
                var ticket = new Chamado { SolicitanteId = otherUser.Id, UnidadeAtendimentoId = unidade.Id, Descricao = "Dados de outra prefeitura", AreaAtendimento = "TI", Natureza = "Software", Modalidade = "Remoto" };
                db.AddRange(prefeitura, outra, usuario, tecnico, inactive, unidade, otherUser, ticket);
                await db.SaveChangesAsync();
            }
            (await client.GetAsync("/api/usuario/chamados")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var request = new LoginRequest("  SOLICITANTE@EXAMPLE.TEST  ", password, prefeitura.Id);
            // O frontend normaliza o e-mail antes da validação do request.
            request = request with { Email = request.Email.Trim().ToLowerInvariant() };
            var response = await client.PostAsJsonAsync("/api/auth/web/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            login.Should().NotBeNull();
            login!.UsuarioId.Should().Be(usuario.Id);
            login.PrefeituraId.Should().Be(prefeitura.Id);
            login.Papel.Should().Be("Usuario");
            login.TecnicoId.Should().BeNull();
            login.AccessToken.Should().NotBeNullOrWhiteSpace();
            login.ExpiraEm.Should().BeAfter(DateTime.UtcNow);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
            var portal = await client.GetAsync("/api/usuario/chamados");
            portal.StatusCode.Should().Be(HttpStatusCode.OK, await portal.Content.ReadAsStringAsync() + " / " + portal.Headers.ToString());
            var payload = await portal.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            payload.GetProperty("total").GetInt32().Should().Be(0);
            (await client.GetAsync("/api/Usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await client.PostAsJsonAsync("/api/auth/web/login", request with { Senha = "wrong" })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.PostAsJsonAsync("/api/auth/web/login", request with { PrefeituraId = outra.Id })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.PostAsJsonAsync("/api/auth/web/login", request with { Email = "inativo@example.test" })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.PostAsJsonAsync("/api/auth/web/login", request with { Email = "tecnico@example.test" })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
            (await client.GetAsync("/api/usuario/chamados")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            foreach (var file in new[] { database, database + "-wal", database + "-shm" })
                if (File.Exists(file)) File.Delete(file);
        }
    }
}
