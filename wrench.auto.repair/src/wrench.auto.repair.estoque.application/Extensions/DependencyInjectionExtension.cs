using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.estoque.application.SortMaps;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddEstoqueApplication(this IServiceCollection services)
        {
            services.ConfigureMediator();
            services.ConfigureAutoMapper();
            services.ConfigureSortMaps();

            return services;
        }

        private static void ConfigureMediator(this IServiceCollection services)
        {
            services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(DependencyInjectionExtension).Assembly));
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
            services.AddScoped<ISortMap<Peca>, PecaSortMap>();
        }
    }
}
