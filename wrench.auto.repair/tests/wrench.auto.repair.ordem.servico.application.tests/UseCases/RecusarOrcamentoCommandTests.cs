using wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase;

namespace wrench.auto.repair.ordem.servico.application.tests.UseCases
{
    public class RecusarOrcamentoCommandTests
    {
        [Fact(DisplayName = "Recusar orçamento command deve ser inválido quando dados incompletos")]
        [Trait("Ordem Serviço", "Application")]
        public void RecusarOrcamentoCommand_DeveSerInvalido_QuandoDadosIncompletos()
        {
            var command = new RecusarOrcamentoCommand(Guid.Empty, string.Empty);

            Assert.False(command.EhValido());
        }

        [Fact(DisplayName = "Recusar orçamento command deve ser válido quando dados corretos")]
        [Trait("Ordem Serviço", "Application")]
        public void RecusarOrcamentoCommand_DeveSerValido_QuandoDadosCorretos()
        {
            var command = new RecusarOrcamentoCommand(Guid.NewGuid(), "Cliente não aprovou o valor.");

            Assert.True(command.EhValido());
        }
    }
}
