using wrench.auto.repair.cadastro.application.Paginacao;
using wrench.auto.repair.cadastro.application.Queries;

namespace wrench.auto.repair.cadastro.application.tests.VeiculoQuery
{
    public class ObterTodosVeiculosQueryTests()
    {
        [Fact(DisplayName = "Obter Todos Veiculos Command Válido")]
        [Trait("Cadastro", "Application")]
        public void ObterTodosVeiculosCommand_ComandoValido_DevePassarNaValidacao()
        {
            var obterTodosVeiculosQuery =
                new ObterTodosVeiculosQuery(new VeiculoRequisicaoPaginada());

            var valido = obterTodosVeiculosQuery.EhValido();

            Assert.True(valido);
        }
    }
}
