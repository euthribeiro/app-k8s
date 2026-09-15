namespace wrench.auto.repair.cadastro.application.Commands.ViewModels
{
    public class CadastrarVeiculoViewModel
    {
        public Guid ClienteId { get; set; }
        public string Marca { get; init; }
        public string Modelo { get; init; }
        public string Cor { get; set; }
        public int AnoFabricacao { get; init; }
        public int AnoModelo { get; init; }
        public string PlacaDoVeiculo { get; init; }
        public string? Descricao { get; init; }
        public DateTime? UltimaRevisao { get; set; }
        public int QuilometragemAtual { get; set; }
    }
}
