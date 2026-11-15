namespace wrench.auto.repair.ordem.servico.application.Queries.ViewModels
{
    public class OrdemServicoHistoricoViewModel
    {
        public string? StatusAnterior { get; init; }
        public string StatusNovo { get; init; } = string.Empty;
        public string? Motivo { get; init; }
        public string? CorrelationId { get; init; }
        public DateTime OcorridoEm { get; init; }
    }
}
