namespace wrench.auto.repair.autenticacao.infra.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string GerarHash(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
        }

        public bool ValidarSenha(string senha, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
    }
}
