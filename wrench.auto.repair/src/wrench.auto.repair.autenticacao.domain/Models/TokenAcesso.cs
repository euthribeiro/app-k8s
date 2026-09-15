namespace wrench.auto.repair.autenticacao.domain.Models
{
    public class TokenAcesso(string token, string username, string role)
    {
        public string Token { get; private set; } = token;
        public string Username { get; private set; } = username;
        public string Role { get; private set; } = role;
    }
}
