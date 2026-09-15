using wrench.auto.repair.cadastro.domain.tests.Fixtures;
using wrench.auto.repair.cadastro.domain.ValueObjects;
using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.cadastro.domain.tests
{
    [Collection(nameof(ClienteCollection))]
    public class EnderecoTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Endereço Validações Devem Retornar Exception")]
        [Trait("Cadastro", "ValueObjects")]
        public void Endereco_Validar_ValidacoesDevemRetornarException()
        {
            Assert.Throws<DomainException>(() => new Endereco(string.Empty, "0", string.Empty, "Meu Bairro", "12345000", "São Paulo", "SP", "Brasil"));
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "", string.Empty, "Meu Bairro", "12345000", "São Paulo", "SP", "Brasil"));
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "0", string.Empty, "", "12345000", "São Paulo", "SP", "Brasil"));
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "0", string.Empty, "Meu Bairro", "", "São Paulo", "SP", "Brasil"));
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "0", string.Empty, "Meu Bairro", "123", "São Paulo", "SP", "Brasil"));
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "0", string.Empty, "Meu Bairro", "12345000", "", "SP", "Brasil"));
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "0", string.Empty, "Meu Bairro", "12345000", "São Paulo", "", "Brasil"));
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "0", string.Empty, "Meu Bairro", "12345000", "São Paulo", "SP", ""));
        }

        [Fact(DisplayName = "Criar Endereço Válido Deve Retornar Endereço Formatado")]
        [Trait("Cadastro", "Domains")]
        public void CriarEndereco_EnderecoValido_DeveRetornarEnderecoFormatado()
        {
            var endereco = new Endereco("Minha Rua", "123", "Casa A", "Meu Bairro", "12345678", "São Paulo", "SP", "Brasil");

            Assert.NotEmpty(endereco.ObterEnderecoFormatado());
        }

        [Fact(DisplayName = "Criar Endereço Cep Inválido Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void CriarEndereco_CepInvalido_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "123", "Casa A", "Meu Bairro", "123", "São Paulo", "SP", "Brasil"));
        }

        [Fact(DisplayName = "Criar Endereço Cep Com Traco Valido Deve Remover Traço")]
        [Trait("Cadastro", "Domains")]
        public void CriarEndereco_CepComTracoValido_DeveRemoverTraco()
        {
            var endereco = new Endereco("Minha Rua", "123", "Casa A", "Meu Bairro", "12345-678", "São Paulo", "SP", "Brasil");

            Assert.Equal("12345678", endereco.Cep);
        }

        [Fact(DisplayName = "Criar Endereço Unidade Federativa Inválida Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void CriarEndereco_UnidadeFederativaInvalida_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new Endereco("Minha Rua", "123", "Casa A", "Meu Bairro", "12345678", "São Paulo", "São Paulo", "Brasil"));
        }
    }
}
