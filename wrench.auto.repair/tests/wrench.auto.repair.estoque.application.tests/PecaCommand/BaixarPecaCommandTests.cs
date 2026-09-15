using wrench.auto.repair.estoque.application.Commands;

namespace wrench.auto.repair.estoque.application.tests.PecaCommand
{
    public class BaixarPecaCommandTests
    {
        [Fact(DisplayName = "Baixar Peça Comando Válido")]
        [Trait("Estoque", "Application")]
        public void Peca_BaixarPecaCommand_ComandoValido()
        {
            var baixarPecaCommand = new BaixarPecaCommand(Guid.NewGuid(), 1);

            var valido = baixarPecaCommand.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Baixar Peça Comando Inválido")]
        [Trait("Estoque", "Application")]
        public void Peca_BaixarPecaCommand_ComandoInvalido()
        {
            var baixarPecaCommand = new BaixarPecaCommand(Guid.Empty, -1);

            var valido = baixarPecaCommand.EhValido();

            Assert.False(valido);
            Assert.Contains(
                BaixarPecaCommandValidator.PecaIdVazioError,
                baixarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
            Assert.Contains(
                BaixarPecaCommandValidator.QuantidadeMinimaError,
                baixarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
