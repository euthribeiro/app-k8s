using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.autenticacao.application.SortMap;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.autenticacao.application.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddAutenticacaoApplication(this IServiceCollection services)
        {
            services.ConfigureMediator();
            services.ConfigureAutoMapper();
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
            services.AddScoped<ISortMap<Usuario>, UsuarioSortMap>();
        }
    }
}
