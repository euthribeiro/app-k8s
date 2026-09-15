using Bogus;
using Bogus.Extensions.Brazil;
using System.Text.RegularExpressions;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.cadastro.domain.ValueObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.cadastro.domain.tests.Fixtures
{

    [CollectionDefinition(nameof(ClienteCollection))]
    public class ClienteCollection : ICollectionFixture<ClienteFixture>
    {

    }

    public class ClienteFixture
    {
        public CpfCnpj GerarCpfValido()
        {
            return new Faker<CpfCnpj>()
                .CustomInstantiator(fk => new CpfCnpj(fk.Person.Cpf()));
        }

        public CpfCnpj GerarCnpjValido()
        {
            return new Faker<CpfCnpj>()
                .CustomInstantiator(fk => new CpfCnpj(fk.Company.Cnpj()));
        }

        public NomeRazaoSocial GerarNomeValido()
        {
            return new Faker<NomeRazaoSocial>()
                .CustomInstantiator(fk => new NomeRazaoSocial(fk.Person.FullName));
        }

        public DataNascimento GerarDataNascimentoValida()
        {
            return new Faker<DataNascimento>()
                .CustomInstantiator(fk => new DataNascimento(fk.Date.Past(18)));
        }

        public Telefone GerarTelefoneValido()
        {
            return new Faker<Telefone>()
                .CustomInstantiator(fk => new Telefone(fk.Phone.PhoneNumber("## 9####-####")));
        }

        public Email GerarEmailValido()
        {
            return new Faker<Email>()
                .CustomInstantiator(fk => new Email(fk.Internet.Email()));
        }

        public Cliente GerarClienteValido()
        {
            var cnpj = GerarCnpjValido();
            var nome = GerarNomeValido();
            var telefone = GerarTelefoneValido();
            var email = GerarEmailValido();
            var endereco = GerarEnderecoValido();

            return new Cliente(cnpj, nome, telefone, email, endereco, DateTime.UtcNow);
        }

        public Endereco GerarEnderecoValido()
        {
            return new Faker<Endereco>("pt_BR")
                .CustomInstantiator(f => new Endereco(
                    f.Address.StreetAddress(),
                    f.Address.BuildingNumber(),
                    f.Address.SecondaryAddress(),
                    f.Address.StreetName() + " " + f.Address.CitySuffix(),
                    Regex.Replace(f.Address.ZipCode(), "\\D", ""),
                    f.Address.City(),
                    f.Address.StateAbbr(),
                    f.Address.Country()
                ));
        }
    }
}
