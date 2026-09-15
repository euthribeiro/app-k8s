using wrench.auto.repair.cadastro.application.Queries;
using wrench.auto.repair.cadastro.application.tests.Fixture;

namespace wrench.auto.repair.cadastro.application.tests.VeiculoQuery
{
    [Collection(nameof(VeiculoCollection))]
    public class ObterVeiculoPorPlacaQueryTests(VeiculoFixture _fixture)
    {
        [Fact(DisplayName = "Obter Veiculo Por Placa Command Válido")]
        [Trait("Cadastro", "Application")]
        public void ObterVeiculoPorPlacaCommand_ComandoValido_DevePassarNaValidacao()
        {
            var placaValida = _fixture.GerarPlacaVeiculoValida();
            var obterVeiculoPorPlacaQuery = new ObterVeiculoPorPlacaQuery(placaValida);

            var valido = obterVeiculoPorPlacaQuery.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Obter Veiculo Por Placa Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void ObterVeiculoPorPlacaCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var obterVeiculoPorPlacaQuery = new ObterVeiculoPorPlacaQuery("");

            var valido = obterVeiculoPorPlacaQuery.EhValido();

            Assert.False(valido);
            Assert.Contains(
                ObterVeiculoPorPlacaQueryValidator.PlacaVaziaError,
                obterVeiculoPorPlacaQuery
                    .ValidationResult
                    .Errors.Select(e => e.ErrorMessage)
            );
            Assert.Contains(
                ObterVeiculoPorPlacaQueryValidator.PlacaInvalidaError,
                obterVeiculoPorPlacaQuery
                    .ValidationResult
                    .Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
