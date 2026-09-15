using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.core.tests.ValueObjects
{
    public class EmailTests
    {
        [Fact(DisplayName = "Criar Email Vazio Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarEmail_EmailVazio_DeveRetornarDomainException()
        {
            var emailVazio = "";

            var exception = Assert.Throws<DomainException>(() =>
            {
                new Email(emailVazio);
            });

            Assert.Equal("E-mail não pode ser vazio", exception.Message);
        }

        [Fact(DisplayName = "Criar Email Inválido Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarEmail_EmailInvalido_DeveRetornarDomainException()
        {
            var emailInvalido = "teste";

            var exception = Assert.Throws<DomainException>(() =>
            {
                new Email(emailInvalido);
            });

            Assert.Equal("E-mail inválido", exception.Message);
        }

        [Theory(DisplayName = "Criar Email Válido")]
        [Trait("Core", "ValueObjects")]
        [InlineData("email@email.com")]
        [InlineData("thiago.ribeiro@fiap.com")]
        [InlineData("bruno_barreto@gmail.com")]
        [InlineData("gabriel_2000@hotmail.com.br")]
        public void CriarEmail_EmailValido_DeveRetornarEnderecoDeEmail(string enderecoDeEmail)
        {
            var email = new Email(enderecoDeEmail);

            Assert.Equal(enderecoDeEmail, email.Endereco);
        }

        [Fact(DisplayName = "Criar Email Válido Obter Dominio")]
        [Trait("Core", "ValueObjects")]
        public void CriarEmail_EmailValido_DeveObterDominioDoEmail()
        {
            var enderecoDeEmailValido = "jose.silva@fiap.com.br";

            var email = new Email(enderecoDeEmailValido);

            Assert.Equal("fiap.com.br", email.Dominio);
        }
    }
}
