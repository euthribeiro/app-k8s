using Amazon.SimpleEmail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.infra.Services.Email;

namespace wrench.auto.repair.infra.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddWrenchInfra(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSimpleEmailService>();
            services.AddScoped<IEmailService, AwsSesEmailService>();
            services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();

            return services;
        }
    }
}
