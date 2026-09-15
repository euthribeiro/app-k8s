using global::AutoMapper;
using Microsoft.Extensions.Logging;
using wrench.auto.repair.ordem.servico.application.AutoMapper;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.domain.ValueObjects;

namespace wrench.auto.repair.ordem.servico.application.tests.AutoMapper
{
    public class OrdemServicoProfileTests
    {
        [Fact(DisplayName = "OrdemServicoProfile deve mapear entidade para view model")]
        [Trait("Ordem Serviço", "Application")]
        public void OrdemServicoProfile_DeveMapearOrdemServicoParaViewModel()
        {
            var loggerFactory = LoggerFactory.Create(_ => { });
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrdemServicoProfile>(), loggerFactory);
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Revisão geral", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            ordem.AdicionarDiagnostico("Troca de óleo", 150m);

            var vm = mapper.Map<OrdemServicoViewModel>(ordem);

            Assert.Equal(ordem.Id, vm.Id);
            Assert.Equal(ordem.ClienteId, vm.ClienteId);
            Assert.Equal(ordem.Descricao.Trim(), vm.Descricao.Trim(), StringComparer.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(vm.Status));
            Assert.Equal(ordem.CalcularValorTotal(), vm.ValorTotal);

            var item = new ItemOrdemServico(Guid.NewGuid(), "Filtro de óleo", 35m, 2);
            var itemVm = mapper.Map<ItemOrdemServicoViewModel>(item);
            Assert.Equal(item.Nome, itemVm.Nome);
            Assert.Equal(2, itemVm.Quantidade);
        }
    }
}
