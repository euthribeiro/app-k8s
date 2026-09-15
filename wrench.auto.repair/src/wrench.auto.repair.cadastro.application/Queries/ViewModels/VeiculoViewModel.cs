namespace wrench.auto.repair.cadastro.application.Queries.ViewModels
{
    public class VeiculoViewModel
    {
        public Guid Id { get; init; }
        public Guid ClienteId { get; init; }
        public string Marca { get; init; }
        public string Modelo { get; init; }
        public string Cor { get; init; }
        public int AnoFabricacao { get; init; }
        public int AnoModelo { get; init; }
        public string PlacaDoVeiculo { get; init; }
        public string? Descricao { get; init; }
        public DateTime? UltimaRevisao { get; set; }
        public int QuilometragemAtual { get; set; }
    }
}
