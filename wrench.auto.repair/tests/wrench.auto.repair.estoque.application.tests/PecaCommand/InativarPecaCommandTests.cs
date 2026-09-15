using wrench.auto.repair.estoque.application.Commands;

namespace wrench.auto.repair.estoque.application.tests.PecaCommand
{
    public class InativarPecaCommandTests
    {
        [Fact(DisplayName = "Inativar Peça Comando Válido")]
        [Trait("Estoque", "Application")]
        public void Peca_InativarPecaCommand_ComandoValido()
        {
            var inativarPecaCommand = new InativarPecaCommand(Guid.NewGuid());

            var valido = inativarPecaCommand.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Inativar Peça Comando Inválido")]
        [Trait("Estoque", "Application")]
        public void Peca_InativarPecaCommand_ComandoInvalido()
        {
            var inativarPecaCommand = new InativarPecaCommand(Guid.Empty);

            var valido = inativarPecaCommand.EhValido();

            Assert.False(valido);
            Assert.Contains(
                InativarPecaCommandValidator.PecaIdVazioError,
                inativarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
