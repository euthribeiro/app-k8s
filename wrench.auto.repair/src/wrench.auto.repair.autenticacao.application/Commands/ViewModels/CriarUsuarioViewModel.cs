namespace wrench.auto.repair.autenticacao.application.Commands.ViewModels
{
    public class CriarUsuarioViewModel
    {
        /// <summary>
        /// E-mail do usuário
        /// </summary>
        public string Email { get; init; }

        /// <summary>
        /// ID do Perfil do Usuário conforme Lista de Perfis
        /// </summary>
        public Guid PerfilId { get; init; }

        /// <summary>
        /// Senha para acesso a API
        /// </summary>
        public string Senha { get; init; }

        /// <summary>
        /// Ativo ou Inativo
        /// </summary>
        public bool Ativo { get; init; }
    }
}
