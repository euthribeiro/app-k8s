namespace wrench.web.api.Models.Requests
{
    public class AtualizarPecaRequest
    {
        public Guid Id { get; init; }
        public string Nome { get; init; }
        public string Descricao { get; init; }
        public double Valor { get; init; }
        public bool Ativo { get; init; }
    }
}
