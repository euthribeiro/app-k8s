using AutoMapper;
using Microsoft.Extensions.Logging;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries.Dtos;
using wrench.auto.repair.estoque.application.AutoMapper;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.tests.AutoMapper
{
    public class PecaProfileTests
    {
        [Fact(DisplayName = "PecaProfile deve mapear entidade para PecaDto")]
        [Trait("Estoque", "Application")]
        public void PecaProfile_DeveMapearParaPecaDto()
        {
            var loggerFactory = LoggerFactory.Create(_ => { });
            var config = new MapperConfiguration(cfg => cfg.AddProfile<PecaProfile>(), loggerFactory);
            var mapper = config.CreateMapper();
            var peca = new Peca("Filtro de óleo", "Original", 45.5, 12, true, DateTime.UtcNow);

            var dto = mapper.Map<PecaDto>(peca);

            Assert.Equal(peca.Id, dto.Id);
            Assert.Equal((decimal)peca.Valor, dto.Valor);
            Assert.Equal(peca.Nome, dto.Nome);
            Assert.Equal(peca.Quantidade, dto.Quantidade);
        }
    }
}
