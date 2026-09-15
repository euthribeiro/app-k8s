using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.autenticacao.domain.Security;
using wrench.auto.repair.autenticacao.infra.Repositories;
using wrench.auto.repair.autenticacao.infra.Security;

namespace wrench.auto.repair.autenticacao.infra.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddAutenticacaoInfra(
            this IServiceCollection services)
        {
            #region segurança
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            #endregion

            #region repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            #endregion

            return services;
        }
    }
}
