using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.infra.Repositories;

namespace wrench.auto.repair.ordem.servico.infra.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddOrdemServicoInfra(this IServiceCollection services)
        {
            services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
            return services;
        }
    }
}
