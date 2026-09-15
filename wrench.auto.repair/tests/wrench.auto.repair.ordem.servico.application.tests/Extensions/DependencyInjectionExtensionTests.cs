using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using wrench.auto.repair.ordem.servico.application.Extensions;

namespace wrench.auto.repair.ordem.servico.application.tests.Extensions
{
    public class DependencyInjectionExtensionTests
    {
        [Fact(DisplayName = "AddOrdemServicoApplication deve registrar serviços de aplicação")]
        [Trait("Ordem Serviço", "Application")]
        public void AddOrdemServicoApplication_DeveRegistrarAutoMapperEMediatR()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOrdemServicoApplication();

            using var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetRequiredService<IMediator>());
        }
    }
}
