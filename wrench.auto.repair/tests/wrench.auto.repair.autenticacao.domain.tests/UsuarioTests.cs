using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.domain.tests.Fixture;
using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.autenticacao.domain.tests
{
    [Collection("UsuarioCollection")]
    public class UsuarioTests(UsuarioFixture _fixture)
    {
        [Fact(DisplayName = "Criar Usuario Dados Vazio Deve Retornar Exception")]
        [Trait("Autenticacao", "Domains")]
        public void CriarUsuario_DadosVazio_DeveRetornarException()
        {
            var email = _fixture.GerarEmail();

            Assert.Throws<DomainException>(() => new Usuario(null, perfilId: Guid.NewGuid(), false, DateTime.UtcNow));
            Assert.Throws<DomainException>(() => new Usuario(email, perfilId: Guid.Empty, false, DateTime.UtcNow));
        }

        [Fact(DisplayName = "Criar Usuario Definir Senha Vazia Deve Retornar Exception")]
        [Trait("Autenticacao", "Domains")]
        public void CriarUsuario_DefinirSenhaVazia_DeveRetornarException()
        {
            var usuario = _fixture.GerarUsuario();

            var exception = Assert.Throws<DomainException>(() => usuario.DefinirSenha(""));
        }

        [Fact(DisplayName = "Criar Usuario Definir Senha Menor Que 60 Caracteres Deve Retornar Exception")]
        [Trait("Autenticacao", "Domains")]
        public void CriarUsuario_DefinirSenhaMenorQue60Caracteres_DeveRetornarException()
        {
            var usuario = _fixture.GerarUsuario();

            var exception = Assert.Throws<DomainException>(() => usuario.DefinirSenha("hash_senha"));
        }

        [Fact(DisplayName = "Criar Usuario Definir Senha Com Sucesso")]
        [Trait("Autenticacao", "Domains")]
        public void CriarUsuario_DefinirSenha_DeveFazerHashDaSenha()
        {
            var usuario = _fixture.GerarUsuario();
            var senha = _fixture.GerarSenha(tamanho: 24);
            var hashSenha = _fixture.GerarHashSenha(senha);

            usuario.DefinirSenha(hashSenha);

            Assert.Equal(hashSenha, usuario.Senha);
        }

        [Fact(DisplayName = "Criar Perfil Com Dados Vazio Deve Retornar Exception")]
        [Trait("Autenticacao", "Domains")]
        public void CriarPerfilDadosVazioDeveRetornarException()
        {
            Assert.Throws<DomainException>(() => new Perfil(nome: "", descricao: "Perfil Administrativo", true, DateTime.UtcNow));
            Assert.Throws<DomainException>(() => new Perfil(nome: "Admin", descricao: "", true, DateTime.UtcNow));
        }

        [Fact(DisplayName = "Alterar Descricao Perfil Nova Descricao Vazia Deve Retornar Exception")]
        [Trait("Autenticacao", "Domains")]
        public void AlterarDescricaoPerfilNovaDescricaoVaziaDeveRetornarException()
        {
            var perfil = _fixture.GerarPerfil();

            Assert.Throws<DomainException>(() => perfil.AlterarDescricao(""));
        }

        [Fact(DisplayName = "Alterar Perfil do Usuario Novo Perfil Deve Alterar")]
        [Trait("Autenticacao", "Domains")]
        public void AlterarPerfilDoUsuario_NovoPerfil_DeveAlterarPerfil()
        {
            var usuario = _fixture.GerarUsuario();
            var perfil = _fixture.GerarPerfil();

            usuario.AlterarPerfil(perfil);

            Assert.Equal(perfil.Id, usuario.PerfilId);
            Assert.Equal(perfil, usuario.Perfil);
        }
    }
}
