using wrench.auto.repair.estoque.application.Commands;

namespace wrench.auto.repair.estoque.application.tests.PecaCommand
{
    public class AtualizarPecaCommandTests
    {
        [Fact(DisplayName = "Atualizar Peça Comando Válido")]
        [Trait("Estoque", "Application")]
        public void Peca_AtualizarPecaCommand_ComandoValido()
        {
            var atualizarPecaCommand =
                new AtualizarPecaCommand(Guid.NewGuid(), "Pneu", "Aro 15", 100.00, true); ;

            var valido = atualizarPecaCommand.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Atualizar Peça Comando Inválido")]
        [Trait("Estoque", "Application")]
        public void Peca_AtualizarPecaCommand_ComandoInvalido()
        {
            var atualizarPecaCommand = new AtualizarPecaCommand(Guid.Empty, "", "", -1, true); ;

            var valido = atualizarPecaCommand.EhValido();

            Assert.False(valido);
            Assert.Contains(
                AtualizarPecaCommandValidator.PecaIdVazioError,
                atualizarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
            Assert.Contains(
                AtualizarPecaCommandValidator.NomeVazioError,
                atualizarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
            Assert.Contains(
                AtualizarPecaCommandValidator.DescricaoVazioError,
                atualizarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
            Assert.Contains(
                AtualizarPecaCommandValidator.NomeVazioError,
                atualizarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
            Assert.Contains(
                AtualizarPecaCommandValidator.ValorMinimoError,
                atualizarPecaCommand.ValidationResult.Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
