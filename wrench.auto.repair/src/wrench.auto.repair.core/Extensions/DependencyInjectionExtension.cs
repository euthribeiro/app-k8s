using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.core.AutoMapper;
using wrench.auto.repair.core.Mediator;

namespace wrench.auto.repair.core.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<IMediatorHandler, MediatorHandler>();

            services.AddTransient(typeof(ResultadoPaginadoConverter<,>));

            services.ConfigureAutoMapper();

            return services;
        }

        private static void ConfigureAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(configAction =>
            {
                configAction.AddMaps(typeof(DependencyInjectionExtension).Assembly);
            });
        }
    }
}
