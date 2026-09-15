using wrench.auto.repair.autenticacao.application.Commands.ViewModels;

namespace wrench.auto.repair.autenticacao.application.tests.Commands.ViewModels
{
    public class CommandViewModelsTests
    {
        [Fact(DisplayName = "View models de comando devem preservar valores")]
        [Trait("Autenticacao", "Application")]
        public void CommandViewModels_DeveExporPropriedades()
        {
            var perfilId = Guid.NewGuid();
            var criar = new CriarUsuarioViewModel
            {
                Email = "u@test.com",
                PerfilId = perfilId,
                Senha = "Xm9#Zp2$Kq7@",
                Ativo = true
            };
            Assert.Equal("u@test.com", criar.Email);
            Assert.Equal(perfilId, criar.PerfilId);
            Assert.Equal("Xm9#Zp2$Kq7@", criar.Senha);
            Assert.True(criar.Ativo);

            var auth = new AutenticarUsuarioViewModel { Email = "a@test.com", Senha = "secret" };
            Assert.Equal("a@test.com", auth.Email);
            Assert.Equal("secret", auth.Senha);

            var primeiro = new PrimeirAcessoUsuarioViewModel { Email = "p@test.com", Senha = "Xm9#Zp2$Kq7@" };
            Assert.Equal("p@test.com", primeiro.Email);
            Assert.Equal("Xm9#Zp2$Kq7@", primeiro.Senha);

            var token = new TokenAcessoViewModel("tok", "user", "Cliente");
            Assert.Equal("tok", token.Token);
            Assert.Equal("user", token.Username);
            Assert.Equal("Cliente", token.Role);
        }
    }
}
