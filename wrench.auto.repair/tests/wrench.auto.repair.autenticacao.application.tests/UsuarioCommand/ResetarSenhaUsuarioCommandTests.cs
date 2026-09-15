using wrench.auto.repair.autenticacao.application.Commands;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioCommand
{
    public class ResetarSenhaUsuarioCommandTests
    {
        [Fact(DisplayName = "Resetar Senha Usuario Command Válido")]
        [Trait("Autenticacao", "Application")]
        public void ResetarSenhaUsuarioCommand_ComandoValido_DevePassarNaValidacao()
        {
            var resetarSenhaUsuarioCommand = new ResetarSenhaUsuarioCommand(Guid.NewGuid());

            var result = resetarSenhaUsuarioCommand.EhValido();

            Assert.True(result);
        }

        [Fact(DisplayName = "Resetar Senha Command Inválido")]
        [Trait("Autenticacao", "Application")]
        public void ResetarSenhaUsuarioCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var resetarSenhaUsuarioCommand = new ResetarSenhaUsuarioCommand(Guid.Empty);

            var result = resetarSenhaUsuarioCommand.EhValido();

            Assert.False(result);
            Assert.Contains(ResetarSenhaUsuarioCommandValidator.UsuarioIdVazio, resetarSenhaUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
        }
    }
}
