using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.tests.Fixture;

namespace wrench.auto.repair.cadastro.application.tests.VeiculoCommand
{
    [Collection(nameof(VeiculoCollection))]
    public class CadastrarVeiculoCommandTests(VeiculoFixture _fixture)
    {
        [Fact(DisplayName = "Cadastrar Veiculo Command Válido")]
        [Trait("Cadastro", "Application")]
        public void CadastrarVeiculoCommand_ComandoValido_DevePassarNaValidacao()
        {
            var clienteId = Guid.NewGuid();
            var marca = "Fiat";
            var modelo = "Uno";
            var anoFabricacao = 2018;
            var anoModelo = 2018;
            var cor = _fixture.GerarCorAleatoria();
            var placaVeiculo = _fixture.GerarPlacaVeiculoValida();
            var cadastrarVeiculoCommand =
                new CadastrarVeiculoCommand(clienteId, marca, modelo, cor, anoFabricacao, anoModelo, placaVeiculo, null, DateTime.UtcNow, 0);

            var valido = cadastrarVeiculoCommand.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Cadastrar Veiculo Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void CadastrarVeiculoCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var cadastrarVeiculoCommand =
                new CadastrarVeiculoCommand(
                    Guid.Empty, "", "", "",
                    0, 0, "", null, null, -1);

            var valido = cadastrarVeiculoCommand.EhValido();

            Assert.False(valido);

            Assert.Contains(
                CadastrarVeiculoCommandValidator.ClienteIdVazioError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.ModeloVazioError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.CorVazioError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.TamanhoMinimoCorError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.MenorAnoDeFabricacaoError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.MenorAnoModeloError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.PlacaVeiculoVazioError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.PlacaVeiculoInvalidoError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));

            Assert.Contains(
                CadastrarVeiculoCommandValidator.QuilometragemInvalidaError,
                cadastrarVeiculoCommand
                    .ValidationResult
                    .Errors
                    .Select(e => e.ErrorMessage));
        }
    }
}
