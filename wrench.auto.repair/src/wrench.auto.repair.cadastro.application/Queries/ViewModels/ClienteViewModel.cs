using wrench.auto.repair.core.Security;

namespace wrench.auto.repair.cadastro.application.Queries.ViewModels
{
    public class ClienteViewModel
    {
        public Guid Id { get; init; }

        [SensitiveData(SensitiveDataType.CpfCnpj)]
        public string Documento { get; init; }

        public string Nome { get; init; }

        public string Telefone { get; init; }

        public string Email { get; init; }

        public DateTime DataCadastro { get; init; }

        public EnderecoViewModel Endereco { get; init; }
    }
}
