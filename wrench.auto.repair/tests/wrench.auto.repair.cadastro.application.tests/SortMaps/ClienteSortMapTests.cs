using wrench.auto.repair.cadastro.application.SortMaps;

namespace wrench.auto.repair.cadastro.application.tests.SortMaps
{
    public class ClienteSortMapTests
    {
        [Fact(DisplayName = "ClienteSortMap deve expor chaves de ordenação")]
        [Trait("Cadastro", "Application")]
        public void ClienteSortMap_DeveConterChavesOrdenacao()
        {
            var map = new ClienteSortMap().Map;

            Assert.Contains("Nome", map.Keys, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("Documento", map.Keys, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("DataCadastro", map.Keys, StringComparer.OrdinalIgnoreCase);
        }
    }
}
