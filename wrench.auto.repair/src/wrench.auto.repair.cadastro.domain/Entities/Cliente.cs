using wrench.auto.repair.cadastro.domain.ValueObjects;
using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.cadastro.domain.Entities
{
    public class Cliente : Entity, IAggregateRoot
    {
        /// <summary>Construtor sem parametros exigido pelo Entity Framework Core para materializar o tipo a partir do banco.</summary>
        protected Cliente() { }

        public Cliente(CpfCnpj documento, NomeRazaoSocial nomeCompleto, Telefone telefone, Email email, Endereco endereco, DateTime dataCadastro)
        {
            Documento = documento;
            Nome = nomeCompleto;
            Telefone = telefone;
            Email = email;
            Endereco = endereco;
            DataCadastro = dataCadastro;

            Validar();
        }

        public CpfCnpj Documento { get; private set; }

        public NomeRazaoSocial Nome { get; private set; }

        public Telefone Telefone { get; private set; }

        public Email Email { get; private set; }

        public DateTime DataCadastro { get; private set; }

        public Endereco Endereco { get; private set; }

        /// <summary>Propriedade de navegacao mapeada pelo Entity Framework Core.</summary>
        public IEnumerable<Veiculo> Veiculos { get; private set; }

        public void AtualizarNome(NomeRazaoSocial nomeCompleto)
        {
            Validacoes.ValidarSeNulo(nomeCompleto, "Nome não pode ser nulo");
            Nome = nomeCompleto;
        }

        public void AtualizarTelefone(Telefone telefone)
        {
            Validacoes.ValidarSeNulo(telefone, "Telefone não pode ser nulo");
            Telefone = telefone;
        }

        public void AtualizarEmail(Email email)
        {
            Validacoes.ValidarSeNulo(email, "E-mail não pode ser nulo");
            Email = email;
        }

        public void AtualizarEndereco(Endereco endereco)
        {
            Validacoes.ValidarSeNulo(endereco, "Endereço não pode ser nulo");
            Endereco = endereco;
        }

        private void Validar()
        {
            Validacoes.ValidarSeNulo(Documento, "Documento não pode ser nulo");
            Validacoes.ValidarSeNulo(Nome, "Nome não pode ser nulo");
            Validacoes.ValidarSeNulo(Telefone, "Telefone não pode ser nulo");
            Validacoes.ValidarSeNulo(Email, "E-mail não pode ser nulo");
            Validacoes.ValidarSeNulo(Endereco, "Endereço não pode ser nulo");
        }
    }
}
