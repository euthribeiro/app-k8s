using wrench.auto.repair.autenticacao.application.Commands;

namespace wrench.auto.repair.autenticacao.application.tests.UsuarioCommand
{
    public class CriarUsuarioCommandTests
    {
        [Fact(DisplayName = "Criar Usuario Command Válido")]
        [Trait("Autenticacao", "Application")]
        public void CriarUsuarioCommand_ComandoEstaValido_DevePassarNaValidacao()
        {
            var criarUsuarioCommand = new CriarUsuarioCommand("email@email.com", "My@Secret123!", Guid.NewGuid(), true);

            var result = criarUsuarioCommand.EhValido();

            Assert.True(result);
        }

        [Fact(DisplayName = "Criar Usuario Command Inválido")]
        [Trait("Autenticacao", "Application")]
        public void CriarUsuarioCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var criarUsuarioCommand = new CriarUsuarioCommand("", "", Guid.Empty, true);

            var result = criarUsuarioCommand.EhValido();

            Assert.False(result);
            Assert.Contains(CriarUsuarioCommandValidator.PerfilVazioErro, criarUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CriarUsuarioCommandValidator.EmailVazioErro, criarUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CriarUsuarioCommandValidator.EmailInvalidoErro, criarUsuarioCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
        }
    }
}
