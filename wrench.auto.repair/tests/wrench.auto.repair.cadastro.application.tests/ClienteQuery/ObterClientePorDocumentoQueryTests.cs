using wrench.auto.repair.cadastro.application.Queries;
using wrench.auto.repair.cadastro.application.tests.Fixture;

namespace wrench.auto.repair.cadastro.application.tests.ClienteQuery
{
    [Collection(nameof(ClienteCollection))]
    public class ObterClientePorDocumentoQueryTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Obter Cliente Por Documento Command Válido")]
        [Trait("Cadastro", "Application")]
        public void ObterClientePorDocumentCommand_ComandoValido_DevePassarNaValidacao()
        {
            var documento = _fixture.GerarDocumentoValido(cpf: false);
            var obterClientePorDocumentQuery = new ObterClientePorDocumentoQuery(documento);

            var valido = obterClientePorDocumentQuery.EhValido();

            Assert.True(valido);
        }

        [Fact(DisplayName = "Obter Cliente Por Documento Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void ObterClientePorDocumentCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var obterClientePorDocumentQuery = new ObterClientePorDocumentoQuery("");

            var valido = obterClientePorDocumentQuery.EhValido();

            Assert.False(valido);
            Assert.Contains(
                ObterClientePorDocumentoQueryValidator.DocumentoVazioError,
                obterClientePorDocumentQuery
                    .ValidationResult
                    .Errors.Select(e => e.ErrorMessage)
            );
            Assert.Contains(
                ObterClientePorDocumentoQueryValidator.DocumentoInvalidoError,
                obterClientePorDocumentQuery
                    .ValidationResult
                    .Errors.Select(e => e.ErrorMessage)
            );
        }
    }
}
