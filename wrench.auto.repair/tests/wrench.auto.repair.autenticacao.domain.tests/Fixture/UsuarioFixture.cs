using Bogus;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.infra.Security;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.domain.tests.Fixture
{
    [CollectionDefinition("UsuarioCollection")]
    public class UsuarioCollection : ICollectionFixture<UsuarioFixture>
    {

    }

    public class UsuarioFixture
    {
        public Email GerarEmail()
        {
            return new Faker<Email>("pt_BR")
                .CustomInstantiator((fk) => new Email(fk.Internet.Email()));
        }

        public Usuario GerarUsuario()
        {
            return new Usuario(GerarEmail(), Guid.NewGuid(), true, DateTime.UtcNow);
        }

        public Perfil GerarPerfil()
        {
            return new Faker<Perfil>("pt_BR")
            .CustomInstantiator((fk) => new Perfil("Admin", fk.Random.Word(), true, fk.Date.Recent()));
        }

        public string GerarSenha(int tamanho)
        {
            return new Faker("pt_BR").Internet.Password(length: tamanho);
        }

        public string GerarHashSenha(string senha)
        {
            return new PasswordHasher().GerarHash(senha);
        }
    }
}
