using wrench.auto.repair.estoque.application.Commands;

namespace wrench.auto.repair.estoque.application.tests.PecaCommand
{
    public class AtivarPecaCommandTests
    {
        [Fact(DisplayName = "Ativar Peça Comando Válido")]
        [Trait("Estoque", "Application")]
        public void Peca_AtivarPecaCommand_ComandoValido()
        {
            var ativarPecaCommand = new AtivarPecaCommand(Guid.NewGuid());

            var valido = ativarPecaCommand.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Ativar Peça Comando Inválido")]
        [Trait("Estoque", "Application")]
        public void Peca_AtivarPecaCommand_ComandoInvalido()
        {
            var ativarPecaCommand = new AtivarPecaCommand(Guid.Empty);

            var valido = ativarPecaCommand.EhValido();

            Assert.False(valido);
            Assert.Contains(
                AtivarPecaCommandValidator.PecaIdVazioError,
                ativarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
