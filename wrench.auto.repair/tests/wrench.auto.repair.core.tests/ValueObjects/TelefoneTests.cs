using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.core.tests.ValueObjects
{
    public class TelefoneTests
    {
        [Fact(DisplayName = "Criar Telefone Vazio Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarTelefone_TelefoneVazio_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new Telefone("", "11", "123456789"));
            Assert.Throws<DomainException>(() => new Telefone("55", "", "123456789"));
            Assert.Throws<DomainException>(() => new Telefone("55", "11", ""));
            Assert.Throws<DomainException>(() => new Telefone("", "", ""));
        }

        [Fact(DisplayName = "Criar Telefone Com Letras e Simbolos Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarTelefone_TelefoneComLetrasESimbolos_DeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new Telefone("+55", "#1", "12345678#"));
        }

        [Fact(DisplayName = "Criar Telefone Valido")]
        [Trait("Core", "ValueObjects")]
        public void CriarTelefone_TelefoneValido_DeveObterTelefoneCompleto()
        {
            var ddi = "55";
            var ddd = "11";
            var numero = "997978989";

            var telefone = new Telefone(ddi, ddd, numero);

            Assert.Equal($"+{ddi}{ddd}{numero}", telefone.ObterTelefone());
        }

        [Fact(DisplayName = "Criar Telefone Sem DDI Deve Completar com DDI Brasileiro")]
        [Trait("Core", "ValueObjects")]
        public void CriarTelefone_TelefoneSemDDI_DevePreencherComDDIBrasileiro()
        {
            var ddd = "11";
            var numero = "997978989";

            var telefone = new Telefone(ddd, numero);

            Assert.Equal($"+55{ddd}{numero}", telefone.ObterTelefone());
        }

        [Fact(DisplayName = "Criar Telefone Estrangeiro Deve Retornar Exception")]
        [Trait("Core", "ValueObjects")]
        public void CriarTelefone_TelefoneEstrangeiro_DeveRetornarException()
        {
            var telefone = "+1 212 555 1234";

            Assert.Throws<DomainException>(() => new Telefone(telefone));
        }

        [Fact(DisplayName = "Criar Telefone Brasileiro Deve Retornar Telefone Completo")]
        [Trait("Core", "ValueObjects")]
        public void CriarTelefone_TelefoneBrasileiro_DeveRetornarTelefoneCompleto()
        {
            var numeroDeTelefone = "+5511981811515";

            var telefone = new Telefone(numeroDeTelefone);

            Assert.Equal(numeroDeTelefone, telefone.ObterTelefone());
        }
    }
}
