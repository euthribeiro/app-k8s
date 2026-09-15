namespace wrench.auto.repair.autenticacao.application.Queries.ViewModels
{
    public class UsuarioViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; init; }
        public PerfilViewModel Perfil { get; init; }
        public bool Ativo { get; init; }
        public DateTime DataCadastro { get; init; }
    }
}
