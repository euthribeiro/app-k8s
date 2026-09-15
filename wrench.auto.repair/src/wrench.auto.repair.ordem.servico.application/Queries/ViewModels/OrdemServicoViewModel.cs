namespace wrench.auto.repair.ordem.servico.application.Queries.ViewModels
{
    public class OrdemServicoViewModel
    {
        public Guid Id { get; init; }
        public Guid ClienteId { get; init; }
        public Guid VeiculoId { get; set; }
        public string Descricao { get; init; }
        public DateTime DataCriacao { get; init; }
        public string? Status { get; init; }
        public decimal ValorServico { get; init; }
        public string? SolucaoProposta { get; init; }
        public DateTime? DataDiagnostico { get; init; }
        public DateTime? DataEnvio { get; init; }
        public string? StatusAprovacao { get; init; }
        public DateTime? DataAprovacaoRecusa { get; init; }
        public DateTime? DataFinalizacao { get; init; }
        public DateTime? DataEntrega { get; init; }
        public List<ItemOrdemServicoViewModel> Pecas { get; init; }
        public decimal ValorTotal { get; init; }
        public double? TempoExecucaoOrdem { get; init; }
    }
}
