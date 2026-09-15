namespace wrench.auto.repair.ordem.servico.application.Queries.ViewModels
{
    public class ItemOrdemServicoViewModel
    {
        public Guid PecaId { get; init; }
        public string Nome { get; init; }
        public decimal ValorUnitario { get; init; }
        public int Quantidade { get; init; }
    }
}
