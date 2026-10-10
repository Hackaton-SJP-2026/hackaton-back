using System.Reflection;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Hackaton.Application.Interface;
using Hackaton.Application.Services;
using Hackaton.Application.DTOs;
using Hackaton.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Hackaton.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var mapsterConfig = TypeAdapterConfig.GlobalSettings;
            mapsterConfig.Scan(assembly);
            services.AddSingleton(mapsterConfig);
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddScoped(typeof(IAppService<,>), typeof(AppService<,>));
            services.AddScoped<IAppService<Chamado, ChamadoDto>, ChamadoAppService>();
            services.AddValidatorsFromAssembly(assembly);
            return services;
        }
    }
}
