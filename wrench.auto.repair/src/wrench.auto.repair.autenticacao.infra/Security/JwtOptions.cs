namespace wrench.auto.repair.autenticacao.infra.Security
{
    public class JwtOptions
    {
        public string Secret { get; init; }
        public string Issuer { get; init; }
        public string Audience { get; init; }
        public int ExpirationMinutes { get; init; }
    }
}
