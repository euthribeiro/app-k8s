using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.cadastro.application.SortMaps;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.cadastro.application.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddCadastroApplication(this IServiceCollection services)
        {
            services.ConfigureAutoMapper();

            services.ConfigureMediator();

            services.ConfigureSortMaps();

            return services;
        }

        private static void ConfigureAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(configAction =>
            {
                configAction.AddMaps(typeof(DependencyInjectionExtension).Assembly);
            });
        }

        private static void ConfigureMediator(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(typeof(DependencyInjectionExtension).Assembly);
            });
        }

        private static void ConfigureSortMaps(this IServiceCollection services)
        {
            services.AddScoped<ISortMap<Cliente>, ClienteSortMap>();
            services.AddScoped<ISortMap<Veiculo>, VeiculoSortMap>();
        }
    }
}
