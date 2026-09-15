using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.ordem.servico.domain.ValueObjects;

namespace wrench.auto.repair.ordem.servico.domain.tests.ValueObjects
{
    public class ItemOrdemServicoTests
    {
        [Fact(DisplayName = "ItemOrdemServico deve criar com dados válidos")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveCriar_QuandoDadosValidos()
        {
            var pecaId = Guid.NewGuid();

            var item = new ItemOrdemServico(pecaId, "Pastilha de freio", 89.90m, 2);

            Assert.Equal(pecaId, item.PecaId);
            Assert.Equal("Pastilha de freio", item.Nome);
            Assert.Equal(89.90m, item.ValorUnitario);
            Assert.Equal(2, item.Quantidade);
        }

        [Fact(DisplayName = "ItemOrdemServico deve falhar quando pecaId for vazio")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveLancar_QuandoPecaIdInvalido()
        {
            Assert.Throws<DomainException>(() =>
                new ItemOrdemServico(Guid.Empty, "Peça", 10m, 1));
        }

        [Fact(DisplayName = "ItemOrdemServico deve falhar quando nome for vazio")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveLancar_QuandoNomeVazio()
        {
            Assert.Throws<DomainException>(() =>
                new ItemOrdemServico(Guid.NewGuid(), "  ", 10m, 1));
        }

        [Fact(DisplayName = "ItemOrdemServico deve falhar quando valor unitário for negativo")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveLancar_QuandoValorUnitarioNegativo()
        {
            Assert.Throws<DomainException>(() =>
                new ItemOrdemServico(Guid.NewGuid(), "Peça", -0.01m, 1));
        }

        [Fact(DisplayName = "ItemOrdemServico deve falhar quando quantidade for menor que 1")]
        [Trait("Ordem Serviço", "Domain")]
        public void Construtor_DeveLancar_QuandoQuantidadeInvalida()
        {
            Assert.Throws<DomainException>(() =>
                new ItemOrdemServico(Guid.NewGuid(), "Peça", 10m, 0));
        }

        [Fact(DisplayName = "AdicionarUnidades deve incrementar quantidade")]
        [Trait("Ordem Serviço", "Domain")]
        public void AdicionarUnidades_DeveSomarQuantidade()
        {
            var item = new ItemOrdemServico(Guid.NewGuid(), "Parafuso", 2m, 3);

            item.AdicionarUnidades(2);

            Assert.Equal(5, item.Quantidade);
        }

        [Fact(DisplayName = "AdicionarUnidades deve falhar quando quantidade adicional for inválida")]
        [Trait("Ordem Serviço", "Domain")]
        public void AdicionarUnidades_DeveLancar_QuandoQuantidadeMenorQueUm()
        {
            var item = new ItemOrdemServico(Guid.NewGuid(), "Porca", 1m, 2);

            Assert.Throws<DomainException>(() => item.AdicionarUnidades(0));
        }

        [Fact(DisplayName = "CalcularValorTotalPeca deve multiplicar unitário pela quantidade")]
        [Trait("Ordem Serviço", "Domain")]
        public void CalcularValorTotalPeca_DeveMultiplicar()
        {
            var item = new ItemOrdemServico(Guid.NewGuid(), "Correia", 30m, 4);

            Assert.Equal(120m, item.CalcularValorTotalPeca());
        }
    }
}
