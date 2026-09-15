using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.Commands.Dtos;
using wrench.auto.repair.cadastro.application.tests.Fixture;

namespace wrench.auto.repair.cadastro.application.tests.ClienteCommand
{
    [Collection(nameof(ClienteCollection))]
    public class CadastrarClienteCommandTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Cadastrar Cliente Command Válido")]
        [Trait("Cadastro", "Application")]
        public void CadastrarClienteCommand_ComandoValido_DevePassarNaValidacao()
        {
            var endereco = _fixture.GerarEnderecoDtoValido();
            var documento = _fixture.GerarDocumentoValido();
            var telefone = _fixture.GerarNumeroTelefoneValido();
            var email = _fixture.GerarEnderecoEmailValido();
            var nome = _fixture.GerarNomeQualquerValido();

            var cadastrarClienteCommand =
                new CadastrarClienteCommand(documento, nome, telefone, email, endereco);

            var result = cadastrarClienteCommand.EhValido();

            Assert.True(result);
        }

        [Fact(DisplayName = "Cadastrar Cliente Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void CadastrarClienteCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var endereco = new EnderecoDto("", "", null, "", "", "", "", "");
            var cadastrarClienteCommand = new CadastrarClienteCommand("", "", "", "", endereco);
            var cadastrarClienteCommand2 = new CadastrarClienteCommand("", "", "", "", null);

            var result = cadastrarClienteCommand.EhValido();
            var result2 = cadastrarClienteCommand2.EhValido();

            Assert.False(result);
            Assert.False(result2);
            Assert.Contains(CadastrarClienteCommandValidator.DocumentoVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.DocumentoInvalidoError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.NomeVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.NomeInvalidoError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.TelefoneVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.TelefoneInvalidoError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.EmailVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.EmailInvalidoError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(CadastrarClienteCommandValidator.EnderecoNuloError, cadastrarClienteCommand2.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.LogradouroVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.BairroVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.CepVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.CepInvalidoError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.CidadeVaziaError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.UnidadeFederativaVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.UnidadeFederativaInvalidaError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.PaisVazioError, cadastrarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
        }
    }
}