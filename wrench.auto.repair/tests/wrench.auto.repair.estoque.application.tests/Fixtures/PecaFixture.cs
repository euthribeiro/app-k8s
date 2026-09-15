using AutoMapper;
using Microsoft.Extensions.Logging;
using wrench.auto.repair.core.AutoMapper;
using wrench.auto.repair.estoque.application.AutoMapper;

namespace wrench.auto.repair.estoque.application.tests.Fixtures
{
    [CollectionDefinition(nameof(PecaCollection))]
    public class PecaCollection : ICollectionFixture<PecaFixture>
    {

    }

    public class PecaFixture
    {
        public IMapper ConfigurarMapeamentoEGerarMapper()
        {
            var loggerFactory = LoggerFactory.Create(builder => { });
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PecaProfile>();
                cfg.AddProfile<ResultadoPaginadoProfile>();
            }, loggerFactory);

            return config.CreateMapper();
        }
    }
}
