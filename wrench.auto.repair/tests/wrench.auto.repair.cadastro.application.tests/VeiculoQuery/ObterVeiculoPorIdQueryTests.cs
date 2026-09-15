using wrench.auto.repair.cadastro.application.Queries;

namespace wrench.auto.repair.cadastro.application.tests.VeiculoQuery
{
    public class ObterVeiculoPorIdQueryTests()
    {
        [Fact(DisplayName = "Obter Veiculo Por Id Command Válido")]
        [Trait("Cadastro", "Application")]
        public void ObterVeiculoPorIdCommand_ComandoValido_DevePassarNaValidacao()
        {
            var veiculoId = Guid.NewGuid();
            var obterVeiculoPorIdQuery = new ObterVeiculoPorIdQuery(veiculoId);

            var valido = obterVeiculoPorIdQuery.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Obter Veiculo Por Id Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void ObterVeiculoPorIdCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var veiculoId = Guid.Empty;
            var obterVeiculoPorIdQuery = new ObterVeiculoPorIdQuery(veiculoId);

            var valido = obterVeiculoPorIdQuery.EhValido();

            Assert.False(valido);
            Assert.Contains(
                ObterVeiculoPorIdQueryValidator.VeiculoIdVazioError,
                obterVeiculoPorIdQuery
                    .ValidationResult
                    .Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
