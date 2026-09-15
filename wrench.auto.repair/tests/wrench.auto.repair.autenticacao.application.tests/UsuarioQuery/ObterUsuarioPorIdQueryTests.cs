using wrench.auto.repair.autenticacao.application.Queries;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioQuery
{
    public class ObterUsuarioPorIdQueryTests
    {
        [Fact(DisplayName = "Obter Usuario Por Id Command Valido")]
        [Trait("Autenticacao", "Application")]
        public void ObterUsuarioPorIdCommand_ComandoValido_DevePassarNaValidacao()
        {
            var obterUsuarioPorIdQuery = new ObterUsuarioPorIdQuery(Guid.NewGuid());

            var valido = obterUsuarioPorIdQuery.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Obter Usuario Por Id Command Inválido")]
        [Trait("Autenticacao", "Application")]
        public void ObterUsuarioPorIdCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var obterUsuarioPorIdQuery = new ObterUsuarioPorIdQuery(Guid.Empty);

            var valido = obterUsuarioPorIdQuery.EhValido();

            Assert.False(valido);
            Assert.Contains(
                ObterUsuarioPorIdQueryValidator.IdUsuarioVazioError,
                obterUsuarioPorIdQuery
                .ValidationResult
                .Errors.Select(e => e.ErrorMessage));
        }
    }
}
