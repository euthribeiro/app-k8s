using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.cadastro.domain.tests.Fixtures;
using wrench.auto.repair.core.DomainObjects;

namespace wrench.auto.repair.cadastro.domain.tests
{
    [Collection(nameof(ClienteCollection))]
    public class ClienteTests(ClienteFixture _fixture)
    {
        [Fact(DisplayName = "Cliente Dados Vazio Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void CriarCliente_DadosVazio_DeveRetornarException()
        {
            var document = _fixture.GerarCpfValido();
            var nome = _fixture.GerarNomeValido();
            var telefone = _fixture.GerarTelefoneValido();
            var email = _fixture.GerarEmailValido();
            var endereco = _fixture.GerarEnderecoValido();

            Assert.Throws<DomainException>(() =>
            {
                new Cliente(null, nome, telefone, email, endereco, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Cliente(document, null, telefone, email, endereco, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Cliente(document, nome, null, email, endereco, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Cliente(document, nome, telefone, null, endereco, DateTime.UtcNow);
            });

            Assert.Throws<DomainException>(() =>
            {
                new Cliente(document, nome, telefone, email, null, DateTime.UtcNow);
            });
        }

        [Fact(DisplayName = "Alterar Nome Nulo Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void AlterarNomeCliente_NomeNulo_DeveRetornarException()
        {
            var cliente = _fixture.GerarClienteValido();

            Assert.Throws<DomainException>(() => cliente.AtualizarNome(null));
        }

        [Fact(DisplayName = "Alterar Nome Valido Deve Atualizar")]
        [Trait("Cadastro", "Domains")]
        public void AlterarNomeCliente_NomeValido_DeveAtualizar()
        {
            var cliente = _fixture.GerarClienteValido();
            var nome = _fixture.GerarNomeValido();

            cliente.AtualizarNome(nome);

            Assert.Equal(nome, cliente.Nome);
        }

        [Fact(DisplayName = "Alterar Telefone Nulo Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void AlterarTelefoneCliente_TelefoneNulo_DeveRetornarException()
        {
            var cliente = _fixture.GerarClienteValido();

            Assert.Throws<DomainException>(() => cliente.AtualizarTelefone(null));
        }

        [Fact(DisplayName = "Alterar Telefone Valido Deve Atualizar")]
        [Trait("Cadastro", "Domains")]
        public void AlterarTelefoneCliente_TelefoneValido_DeveAtualizar()
        {
            var cliente = _fixture.GerarClienteValido();
            var telefone = _fixture.GerarTelefoneValido();

            cliente.AtualizarTelefone(telefone);

            Assert.Equal(telefone, cliente.Telefone);
        }

        [Fact(DisplayName = "Alterar Email Nulo Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void AlterarEmailCliente_EmailNulo_DeveRetornarException()
        {
            var cliente = _fixture.GerarClienteValido();

            Assert.Throws<DomainException>(() => cliente.AtualizarEmail(null));
        }

        [Fact(DisplayName = "Alterar Email Valido Deve Atualizar")]
        [Trait("Cadastro", "Domains")]
        public void AlterarEmailCliente_EmailValido_DeveAtualizar()
        {
            var cliente = _fixture.GerarClienteValido();
            var email = _fixture.GerarEmailValido();

            cliente.AtualizarEmail(email);

            Assert.Equal(email, cliente.Email);
        }

        [Fact(DisplayName = "Alterar Endereco Nulo Deve Retornar Exception")]
        [Trait("Cadastro", "Domains")]
        public void AlterarEnderecoCliente_EnderecoNulo_DeveRetornarException()
        {
            var cliente = _fixture.GerarClienteValido();

            Assert.Throws<DomainException>(() => cliente.AtualizarEndereco(null));
        }

        [Fact(DisplayName = "Alterar Endereco Valido Deve Atualizar")]
        [Trait("Cadastro", "Domains")]
        public void AlterarEnderecoCliente_EnderecoValido_DeveAtualizar()
        {
            var cliente = _fixture.GerarClienteValido();
            var endereco = _fixture.GerarEnderecoValido();

            cliente.AtualizarEndereco(endereco);

            Assert.Equal(endereco, cliente.Endereco);
        }
    }
}
