using System.Linq.Expressions;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.cadastro.application.SortMaps
{

    public class VeiculoSortMap : ISortMap<Veiculo>
    {
        public Dictionary<string, Expression<Func<Veiculo, object?>>> Map => new(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Marca"] = c => c.Marca,
            ["Modelo"] = c => c.Modelo,
            ["Cor"] = c => c.Cor,
            ["AnoFabricacao"] = c => c.AnoFabricacao,
            ["AnoModelo"] = c => c.AnoModelo,
            ["PlacaDoVeiculo"] = c => c.PlacaDoVeiculo,
            ["UltimaRevisao"] = c => c.UltimaRevisao,
            ["QuilometragemAtual"] = c => c.QuilometragemAtual,
            ["DataCadastro"] = c => c.DataCadastro
        };
    }
}
