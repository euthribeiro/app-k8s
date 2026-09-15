using wrench.auto.repair.autenticacao.application.Commands;
using wrench.auto.repair.autenticacao.application.tests.Fixtures;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioCommand
{
    [Collection(nameof(UsuarioCollection))]
    public class PrimeiroAcessoUsuarioCommandTests(UsuarioFixture _fixture)
    {
        [Fact(DisplayName = "Primeiro Acesso Usuario Command Válido")]
        [Trait("Autenticacao", "Application")]
        public void PrimeiroAcessoUsuarioCommand_ComandoValido_DevePassarNaValidacao()
        {
            var email = _fixture.GerarEmail();
            var primeiroAcessoUsuarioCommand = new PrimeiroAcessoUsuarioCommand(email, "My@Secret@123!");

            var result = primeiroAcessoUsuarioCommand.EhValido();

            Assert.True(result);
        }

        [Fact(DisplayName = "Primeiro Acesso Usuario Command Inválido")]
        [Trait("Autenticacao", "Application")]
        public void PrimeiroAcessoUsuarioCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var primeiroAcessoUsuarioCommand = new PrimeiroAcessoUsuarioCommand("", "");

            var result = primeiroAcessoUsuarioCommand.EhValido();

            Assert.False(result);
            Assert.Contains(PrimeiroAcessoUsuarioCommandValidator.EmailVazioErro, primeiroAcessoUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(PrimeiroAcessoUsuarioCommandValidator.EmailInvalidoErro, primeiroAcessoUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
        }
    }
}
