using wrench.auto.repair.autenticacao.application.Commands;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioCommand
{
    public class InativarUsuarioCommandTests
    {
        [Fact(DisplayName = "Inativar Usuario Command Válido")]
        [Trait("Autenticacao", "Application")]
        public void InativarUsuarioCommand_ComandoValido_DevePassarNaValidacao()
        {
            var inativarUsuarioCommand = new InativarUsuarioCommand(Guid.NewGuid());

            var result = inativarUsuarioCommand.EhValido();

            Assert.True(result);
        }

        [Fact(DisplayName = "Inativar Usuario Command Inválido")]
        [Trait("Autenticacao", "Application")]
        public void InativarUsuarioCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var inativarUsuarioCommand = new InativarUsuarioCommand(Guid.Empty);

            var result = inativarUsuarioCommand.EhValido();

            Assert.False(result);
            Assert.Contains(InativarUsuarioCommandValidator.UsuarioIdVazio, inativarUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
        }
    }
}
