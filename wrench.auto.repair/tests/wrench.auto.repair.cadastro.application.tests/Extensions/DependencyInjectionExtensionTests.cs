using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using wrench.auto.repair.cadastro.application.Extensions;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.cadastro.application.tests.Extensions
{
    public class DependencyInjectionExtensionTests
    {
        [Fact(DisplayName = "AddCadastroApplication deve registrar sort maps")]
        [Trait("Cadastro", "Application")]
        public void AddCadastroApplication_DeveRegistrarSortMaps()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddCadastroApplication();

            using var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetRequiredService<ISortMap<Cliente>>());
            Assert.NotNull(provider.GetRequiredService<ISortMap<Veiculo>>());
        }
    }
}
