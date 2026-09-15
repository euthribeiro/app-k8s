using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.infra.Repositories;

namespace wrench.auto.repair.cadastro.infra.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddCadastroInfra(this IServiceCollection services)
        {
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IVeiculoRepository, VeiculoRepository>();

            return services;
        }
    }
}
