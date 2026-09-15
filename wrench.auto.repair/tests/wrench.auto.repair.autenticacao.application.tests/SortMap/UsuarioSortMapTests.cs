using wrench.auto.repair.autenticacao.application.SortMap;

namespace wrench.auto.repair.autenticacao.application.tests.SortMap
{
    public class UsuarioSortMapTests
    {
        [Fact(DisplayName = "UsuarioSortMap deve expor chaves de ordenação esperadas")]
        [Trait("Autenticacao", "Application")]
        public void UsuarioSortMap_DeveConterChavesOrdenacao()
        {
            var map = new UsuarioSortMap().Map;

            Assert.Contains("Perfil", map.Keys, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("DataCadastro", map.Keys, StringComparer.OrdinalIgnoreCase);
        }
    }
}
