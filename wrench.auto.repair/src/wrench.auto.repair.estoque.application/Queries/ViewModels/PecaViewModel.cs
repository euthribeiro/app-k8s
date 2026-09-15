namespace wrench.auto.repair.estoque.application.Queries.ViewModels
{
    public class PecaViewModel
    {
        public Guid Id { get; init; }
        public string Nome { get; init; }
        public string Descricao { get; init; }
        public double Valor { get; init; }
        public double Quantidade { get; init; }
        public bool Ativo { get; init; }
    }
}
