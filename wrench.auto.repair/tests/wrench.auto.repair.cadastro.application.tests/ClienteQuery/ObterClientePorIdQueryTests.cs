using wrench.auto.repair.cadastro.application.Queries;
using wrench.auto.repair.cadastro.application.tests.Fixture;

namespace wrench.auto.repair.cadastro.application.tests.ClienteQuery
{
    [Collection(nameof(ClienteCollection))]
    public class ObterClientePorIdQueryTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Obter Cliente Por Id Command Válido")]
        [Trait("Cadastro", "Application")]
        public void ObterClientePorIdCommand_ComandoValido_DevePassarNaValidacao()
        {
            var clienteId = Guid.NewGuid();
            var obterClientePorIdQuery = new ObterClientePorIdQuery(clienteId);

            var valido = obterClientePorIdQuery.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Obter Cliente Por Id Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void ObterClientePorIdCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var clienteIdInvalido = Guid.Empty;
            var obterClientePorIdQuery = new ObterClientePorIdQuery(clienteIdInvalido);

            var valido = obterClientePorIdQuery.EhValido();

            Assert.False(valido);
            Assert.Contains(
                ObterClientePorIdQueryValidator.ClienteIdVazio,
                obterClientePorIdQuery
                    .ValidationResult
                    .Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
