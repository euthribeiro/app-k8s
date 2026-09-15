namespace wrench.auto.repair.autenticacao.infra.Security
{
    public interface IPasswordHasher
    {
        bool ValidarSenha(string senha, string hash);
        string GerarHash(string senha);
    }
}
