using wrench.auto.repair.estoque.application.Paginacao;
using wrench.auto.repair.estoque.application.Queries;

namespace wrench.auto.repair.estoque.application.tests.PecaQuery
{
    public class ObterTodasPecasQueryTests
    {
        [Fact(DisplayName = "Obter Todas Peças Query Válida")]
        [Trait("Estoque", "Application")]
        public void Peca_ObterTodasPecasQuery_QueryValida()
        {
            var obterTodasPecasQuery = new ObterTodasPecasQuery(new PecaRequisicaoPaginada());

            var valido = obterTodasPecasQuery.EhValido();

            Assert.True(valido);
        }
    }
}
