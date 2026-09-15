using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.core.tests.ValueObjects
{
    public class NomeCompletoTests
    {
        [Trait(name: "Core", value: "ValueObjects")]
        [Fact(DisplayName = "Criar Nome Completo Vazio Deve Retornar Domain Exception")]
        public void CriarNome_NomeVazio_DeveRetornarDomainException()
        {
            Assert.Throws<DomainException>(() => new NomeRazaoSocial(""));
        }

        [Trait(name: "Core", value: "ValueObjects")]
        [Fact(DisplayName = "Criar Nome Nome Único Deve Retornar Domain Exception")]
        public void CriarNome_NomeUnico_DeveRetornarDomainException()
        {
            Assert.Throws<DomainException>(() => new NomeRazaoSocial("Matheus"));
        }

        [Trait(name: "Core", value: "ValueObjects")]
        [Fact(DisplayName = "Criar Nome Valido Deve Criar Nome")]
        public void CriarNome_NomeValido_DeveCriarComSucesso()
        {
            var nomeRazaoSocial = new NomeRazaoSocial("Jose Antonio da Silva");

            Assert.Equal("Jose Antonio da Silva", nomeRazaoSocial.Nome, true);
        }

        [Trait(name: "Core", value: "ValueObjects")]
        [Fact(DisplayName = "Criar Razão Social Deve Criar Com Sucesso")]
        public void CriarRazaoSocial_NomeValido_DeveCriarComSucesso()
        {
            var nomeRazaoSocial = new NomeRazaoSocial("EMPRESA DA SILVA & SILVA 3 LTDA");

            Assert.Equal("EMPRESA DA SILVA & SILVA 3 LTDA", nomeRazaoSocial.Nome, true);
        }

        [Trait(name: "Core", value: "ValueObjects")]
        [Fact(DisplayName = "Criar Nome Com Espaços Extras Deve Remover Espaços")]
        public void CriarNome_NomeComEspacosExtras_DeveRemoverEspacos()
        {
            var nomeRazaoSocial = new NomeRazaoSocial("   Jose da Silva    ");

            Assert.Equal("JOSE DA SILVA", nomeRazaoSocial.Nome, true);
        }

        [Trait(name: "Core", value: "ValueObjects")]
        [Fact(DisplayName = "Criar Nome Minusculo Deve Transformar em Maiusculo")]
        public void CriarNome_NomeMinusculo_DeveTransformarEmMaiusculo()
        {
            var nomeRazaoSocial = new NomeRazaoSocial("jose da silva");

            Assert.Equal("JOSE DA SILVA", nomeRazaoSocial.Nome);
        }
    }
}
