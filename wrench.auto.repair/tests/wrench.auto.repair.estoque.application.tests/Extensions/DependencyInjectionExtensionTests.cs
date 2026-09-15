using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.estoque.application.Extensions;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.tests.Extensions
{
    public class DependencyInjectionExtensionTests
    {
        [Fact(DisplayName = "AddEstoqueApplication deve registrar sort map e MediatR")]
        [Trait("Estoque", "Application")]
        public void AddEstoqueApplication_DeveRegistrarServicos()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddEstoqueApplication();

            using var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetRequiredService<ISortMap<Peca>>());
            Assert.NotNull(provider.GetRequiredService<IMediator>());
            var mapper = provider.GetRequiredService<IMapper>();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
