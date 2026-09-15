namespace wrench.auto.repair.autenticacao.application.Commands.ViewModels
{
    public class TokenAcessoViewModel(string token, string username, string role)
    {
        public string Token { get; private set; } = token;
        public string Username { get; private set; } = username;
        public string Role { get; private set; } = role;
    }
}
