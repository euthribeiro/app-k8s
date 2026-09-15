using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.tests.Fixture;

namespace wrench.auto.repair.cadastro.application.tests.VeiculoCommand
{
    [Collection(nameof(VeiculoCollection))]
    public class AtualizarVeiculoCommandTests(VeiculoFixture _fixture)
    {
        [Fact(DisplayName = "Atualizar Veiculo Command Válido")]
        [Trait("Cadastro", "Application")]
        public void AtualizarVeiculoCommand_ComandoValido_DevePassarNaValidacao()
        {
            var clienteId = Guid.NewGuid();
            var veiculoId = Guid.NewGuid();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _fixture.GerarCorAleatoria();
            var placaVeiculo = _fixture.GerarPlacaVeiculoValida();
            var atualizarVeiculoCommand =
                new AtualizarVeiculoCommand(veiculoId, clienteId, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);

            var valido = atualizarVeiculoCommand.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Atualizar Veiculo Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void AtualizarVeiculoCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var atualizarVeiculoCommand =
                new AtualizarVeiculoCommand(
                    Guid.Empty, Guid.Empty, "", "",
                    "", 0, 0, "", null, null, -1);

            var valido = atualizarVeiculoCommand.EhValido();

            Assert.False(valido);
            Assert.Contains(
                AtualizarVeiculoCommandValidator.VeiculoIdVazioError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.ClienteIdVazioError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.ModeloVazioError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.CorVazioError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.TamanhoMinimoCorError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.MenorAnoDeFabricacaoError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.MenorAnoModeloError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.PlacaVeiculoVazioError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.PlacaVeiculoInvalidoError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                AtualizarVeiculoCommandValidator.QuilometragemInvalidaError,
                atualizarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));
        }
    }
}
