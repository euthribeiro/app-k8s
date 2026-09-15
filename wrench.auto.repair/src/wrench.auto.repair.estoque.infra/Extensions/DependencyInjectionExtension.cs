using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.estoque.domain.Data;
using wrench.auto.repair.estoque.infra.Repositories;

namespace wrench.auto.repair.estoque.infra.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddEstoqueInfra(this IServiceCollection services)
        {
            services.AddScoped<IPecaRepository, PecaRepository>();

            return services;
        }
    }
}
