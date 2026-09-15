using wrench.auto.repair.cadastro.application.Commands;
using wrench.auto.repair.cadastro.application.Commands.Dtos;
using wrench.auto.repair.cadastro.application.tests.Fixture;

namespace wrench.auto.repair.cadastro.application.tests.ClienteCommand
{
    [Collection(nameof(ClienteCollection))]
    public class AtualizarClienteCommandTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Atualizar Cliente Command Válido")]
        [Trait("Cadastro", "Application")]
        public void AtualizarClienteCommand_ComandoValido_DevePassarNaValidacao()
        {
            var endereco = _fixture.GerarEnderecoDtoValido();
            var atualizarClienteCommand = new AtualizarClienteCommand(
                Guid.NewGuid(), "Jose da Silva", "+5511987654231", "jose.silva@fiap.com.br", endereco);

            var result = atualizarClienteCommand.EhValido();

            Assert.True(result);
        }

        [Fact(DisplayName = "Atualizar Cliente Command Inválido")]
        [Trait("Cadastro", "Application")]
        public void AtualizarClienteCommand_ComandoInvalido_NaoDevePassarNaValidacao()
        {
            var endereco = new EnderecoDto("", "", null, "", "", "", "", "");
            var atualizarClienteCommand = new AtualizarClienteCommand(Guid.Empty, "", "", "", endereco);

            var result = atualizarClienteCommand.EhValido();

            Assert.False(result);
            Assert.Contains(AtualizarClienteCommandValidator.ClienteIdError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(AtualizarClienteCommandValidator.NomeVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(AtualizarClienteCommandValidator.NomeInvalidoError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(AtualizarClienteCommandValidator.TelefoneVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(AtualizarClienteCommandValidator.TelefoneInvalidoError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(AtualizarClienteCommandValidator.EmailVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(AtualizarClienteCommandValidator.EmailInvalidoError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.LogradouroVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.BairroVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.CepVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.CepInvalidoError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.CidadeVaziaError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.UnidadeFederativaVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.UnidadeFederativaInvalidaError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
            Assert.Contains(EnderecoDtoValidator.PaisVazioError, atualizarClienteCommand.ValidationResult.Errors.Select(e => e.ErrorMessage));
        }
    }
}
