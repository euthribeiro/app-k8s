using wrench.auto.repair.estoque.application.SortMaps;

namespace wrench.auto.repair.estoque.application.tests.SortMaps
{
    public class PecaSortMapTests
    {
        [Fact(DisplayName = "PecaSortMap deve expor chaves de ordenação")]
        [Trait("Estoque", "Application")]
        public void PecaSortMap_DeveConterChavesOrdenacao()
        {
            var map = new PecaSortMap().Map;

            Assert.Contains("Nome", map.Keys, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("Valor", map.Keys, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("Quantidade", map.Keys, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("DataCadastro", map.Keys, StringComparer.OrdinalIgnoreCase);
        }
    }
}
