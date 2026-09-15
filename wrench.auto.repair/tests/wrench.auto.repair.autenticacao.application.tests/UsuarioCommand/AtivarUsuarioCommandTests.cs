using wrench.auto.repair.autenticacao.application.Commands;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioCommand
{
    public class AtivarUsuarioCommandTests
    {
        [Fact(DisplayName = "Ativar Usuario Command Válido")]
        [Trait("Autenticacao", "Application")]
        public void AtivarUsuarioCommand_ComandoValido_DevePassarNaValidacao()
        {
            var ativarUsuarioCommand = new AtivarUsuarioCommand(Guid.NewGuid());

            var result = ativarUsuarioCommand.EhValido();

            Assert.True(result);
        }

        [Fact(DisplayName = "Ativar Usuario Command Inválido")]
        [Trait("Autenticacao", "Application")]
        public void AtivarUsuarioCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var ativarUsuarioCommand = new AtivarUsuarioCommand(Guid.Empty);

            var result = ativarUsuarioCommand.EhValido();

            Assert.False(result);
            Assert.Contains(AtivarUsuarioCommandValidator.UsuarioIdVazio, ativarUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
        }
    }
}
