using wrench.auto.repair.estoque.application.Queries;

namespace wrench.auto.repair.estoque.application.tests.PecaQuery
{
    public class ConsultarPecaPorIdQueryTests
    {
        [Fact(DisplayName = "Consulta Peça Por Id Query Válida")]
        [Trait("Estoque", "Application")]
        public void Peca_ConsultaPecaPorIdQuery_QueryValida()
        {
            var consultarPecaPorIdQuery = new ConsultarPecaPorIdQuery(Guid.NewGuid());

            var valido = consultarPecaPorIdQuery.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Consulta Peça Por Id Query Inválida")]
        [Trait("Estoque", "Application")]
        public void Peca_ConsultaPecaPorIdQuery_QueryInvalida()
        {
            var consultarPecaPorIdQuery = new ConsultarPecaPorIdQuery(Guid.Empty);

            var valido = consultarPecaPorIdQuery.EhValido();

            Assert.False(valido);
            Assert.Contains(
                ConsultarPecaPorIdQueryValidator.PecaIdVazioError,
                consultarPecaPorIdQuery.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
