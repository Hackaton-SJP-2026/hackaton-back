using Hackaton.Domain.Interface;
using Hackaton.Infrastructure.Data;
using Hackaton.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hackaton.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            string? databaseBasePath = null)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("ConnectionString 'DefaultConnection' não configurada.");

            var sqlite = new SqliteConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(sqlite.DataSource))
                throw new InvalidOperationException("SQLite exige Data Source na connection string.");

            if (sqlite.Mode != SqliteOpenMode.Memory && sqlite.DataSource != ":memory:")
            {
                // Caminho previsível, independente do diretório do terminal.
                sqlite.DataSource = Path.GetFullPath(sqlite.DataSource,
                    databaseBasePath ?? Directory.GetCurrentDirectory());
                Directory.CreateDirectory(Path.GetDirectoryName(sqlite.DataSource)!);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqlite.ConnectionString));
            services.AddMemoryCache();
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            return services;
        }
    }
}
