namespace wrench.auto.repair.cadastro.application.Commands.ViewModels
{
    public class AtualizarClienteViewModel
    {
        public Guid ClienteId { get; init; }
        public string Nome { get; init; }
        public string Telefone { get; init; }
        public string Email { get; init; }
        public CriarEditarEnderecoViewModel? Endereco { get; init; }
    }
}
