using System.Text;
using Hackaton.Application;
using Hackaton.Domain;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure;
using HackatonApi.Auth;
using HackatonApi.Middlewares;
using HackatonApi.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Hospedagens de containers frequentemente fornecem a porta por PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    if (!int.TryParse(port, out var number) || number < 1 || number > 65535)
        throw new InvalidOperationException("PORT deve ser uma porta entre 1 e 65535.");
    builder.WebHost.UseUrls($"http://0.0.0.0:{number}");
}
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<ApplyReadOnlyAttributeSchemaFilter>();
    options.OperationFilter<HideReadOnlyInRequestBodyFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token JWT, sem o prefixo Bearer. Login ainda não implementado."
    });
    // Somente ações protegidas recebem requisito de token na documentação.
    options.OperationFilter<AuthorizationOperationFilter>();
});
builder.Services.AddDomain();
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserResolver, HttpContextUserResolver>();

var persistenceEnabled = builder.Configuration.GetValue<bool>("Persistence:Enabled");
if (persistenceEnabled)
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

if (builder.Configuration.GetValue<bool>("Authentication:Enabled"))
{
    var issuer = builder.Configuration["JwtSettings:Issuer"];
    var audience = builder.Configuration["JwtSettings:Audience"];
    var key = builder.Configuration["JwtSettings:SigningKey"];
    if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience)
        || string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        throw new InvalidOperationException("JWT exige Issuer, Audience e SigningKey com pelo menos 32 bytes.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero
            };
        });
}
else
{
    builder.Services.AddAuthentication(DisabledAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DisabledAuthenticationHandler>(
            DisabledAuthenticationHandler.SchemeName, _ => { });
}
builder.Services.AddAuthorization();
var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if (builder.Configuration.GetValue("Http:RedirectToHttps", true))
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/api/status", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous().WithTags("Status");
app.Run();

// Permite hospedar a API em testes de integração, sem endpoint de negócio.
public partial class Program { }
