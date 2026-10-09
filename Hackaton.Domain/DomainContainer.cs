using Hackaton.Domain.Interface;
using Hackaton.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hackaton.Domain
{
    public static class DomainContainer
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddScoped(typeof(IDomainService<>), typeof(DomainService<>));
            return services;
        }
    }
}
