using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.domain.tests
{
    public class PecaTests
    {
        [Fact(DisplayName = "Criar Peça Dados Inválidos")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_DadosInvalidos_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new Peca("", "Aro 15", 150, 10, true, DateTime.UtcNow));
            Assert.Throws<DomainException>(() => new Peca("Pneu", "", 150, 10, true, DateTime.UtcNow));
            Assert.Throws<DomainException>(() => new Peca("Pneu", "Aro 15", -1, 10, true, DateTime.UtcNow));
            Assert.Throws<DomainException>(() => new Peca("Pneu", "Aro 15", 150, -1, true, DateTime.UtcNow));
        }

        [Fact(DisplayName = "Criar Peça Dados Válidos")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_DadosValido_NehumaExceptionDeveSerRetornada()
        {
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            Assert.NotNull(peca);
        }

        [Fact(DisplayName = "Criar Peça Alterar Descrição Para Vazio")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_AlterarDescricaoParaVazio_DeveRetornarException()
        {
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => peca.AlterarDescricao(""));
        }

        [Fact(DisplayName = "Criar Peça Alterar Descrição Válida")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_AlterarDescricaoValida_NehumaExceptionDeveSerRetornada()
        {
            var novaDescricao = "Aro 14";
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            peca.AlterarDescricao(novaDescricao);

            Assert.Equal(novaDescricao, peca.Descricao, ignoreCase: true);
        }

        [Fact(DisplayName = "Criar Peça Alterar Valor Inválido")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_AlterarValorInvalido_DeveRetornarException()
        {
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => peca.AlterarValor(-1));
        }

        [Fact(DisplayName = "Criar Peça Alterar Valor Válido")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_AlterarValorValido_NehumaExceptionDeveSerRetornada()
        {
            var novoValor = 160.00;
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            peca.AlterarValor(novoValor);

            Assert.Equal(novoValor, peca.Valor);
        }

        [Fact(DisplayName = "Criar Peça Repor Estoque Quantidade Inválida")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_ReporEstoqueComQuantidadeInvalida_DeveRetornarException()
        {
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => peca.ReporEstoque(-5));
        }

        [Fact(DisplayName = "Criar Peça Repor Estoque Quantidade Válida")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_ReporEstoqueComQuantidadeInvalida_NehumaExceptionDeveSerRetornada()
        {
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            peca.ReporEstoque(5);

            Assert.Equal(15, peca.Quantidade);
        }

        [Fact(DisplayName = "Criar Peça Baixar Estoque Quantidade Inválida")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_BaixarEstoqueComQuantidadeInvalida_DeveRetornarException()
        {
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            Assert.Throws<DomainException>(() => peca.BaixarEstoque(-5));
        }

        [Fact(DisplayName = "Criar Peça Baixar Estoque Quantidade Válida")]
        [Trait("Estoque", "Domains")]
        public void CriarPecas_BaixarEstoqueComQuantidadeInvalida_NehumaExceptionDeveSerRetornada()
        {
            var peca = new Peca("Pneu", "Aro 15", 150, 10, true, DateTime.UtcNow);

            peca.BaixarEstoque(5);

            Assert.Equal(5, peca.Quantidade);
        }
    }
}
