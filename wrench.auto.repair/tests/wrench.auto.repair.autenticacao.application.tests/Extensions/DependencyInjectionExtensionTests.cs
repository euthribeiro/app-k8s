using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.autenticacao.application.Extensions;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.autenticacao.application.tests.Extensions
{
    public class DependencyInjectionExtensionTests
    {
        [Fact(DisplayName = "AddAutenticacaoApplication deve registrar sort map de usuário")]
        [Trait("Autenticacao", "Application")]
        public void AddAutenticacaoApplication_DeveRegistrarUsuarioSortMap()
        {
            var services = new ServiceCollection();
            services.AddAutenticacaoApplication();

            using var provider = services.BuildServiceProvider();
            var sortMap = provider.GetRequiredService<ISortMap<Usuario>>();

            Assert.NotNull(sortMap);
            Assert.NotEmpty(sortMap.Map);
        }
    }
}
