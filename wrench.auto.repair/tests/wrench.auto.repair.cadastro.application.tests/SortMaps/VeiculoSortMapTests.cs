using wrench.auto.repair.cadastro.application.SortMaps;

namespace wrench.auto.repair.cadastro.application.tests.SortMaps
{
    public class VeiculoSortMapTests
    {
        [Fact(DisplayName = "VeiculoSortMap deve expor chaves de ordenação")]
        [Trait("Cadastro", "Application")]
        public void VeiculoSortMap_DeveConterChavesOrdenacao()
        {
            var map = new VeiculoSortMap().Map;

            Assert.Contains("Marca", map.Keys, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("PlacaDoVeiculo", map.Keys, StringComparer.OrdinalIgnoreCase);
        }
    }
}
