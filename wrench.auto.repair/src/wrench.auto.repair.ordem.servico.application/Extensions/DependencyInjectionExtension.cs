using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.ordem.servico.application.Metrics;
using wrench.auto.repair.ordem.servico.application.SortMaps;
using wrench.auto.repair.ordem.servico.domain.Entities;

namespace wrench.auto.repair.ordem.servico.application.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddOrdemServicoApplication(this IServiceCollection services)
        {
            services.ConfigureMediator();
            services.ConfigureAutoMapper();
            services.ConfigureSortMaps();
            services.AddHostedService<OrdemServicoMetrics>();

            return services;
        }


        private static void ConfigureMediator(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(typeof(DependencyInjectionExtension).Assembly);
                config.AddOpenBehavior(typeof(FalhaProcessamentoOrdemServicoBehavior<,>));
            });
        }

        private static void ConfigureAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(configAction =>
            {
                configAction.AddMaps(typeof(DependencyInjectionExtension).Assembly);
            });
        }

        private static void ConfigureSortMaps(this IServiceCollection services)
        {
            services.AddScoped<ISortMap<OrdemServico>, OrdemServicoSortMap>();
        }
    }
}
