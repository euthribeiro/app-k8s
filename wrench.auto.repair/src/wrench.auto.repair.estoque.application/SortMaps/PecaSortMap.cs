using System.Linq.Expressions;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.SortMaps
{
    public class PecaSortMap : ISortMap<Peca>
    {
        public Dictionary<string, Expression<Func<Peca, object?>>> Map => new(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Nome"] = c => c.Nome,
            ["Valor"] = c => c.Valor,
            ["Quantidade"] = c => c.Quantidade,
            ["DataCadastro"] = c => c.DataCadastro
        };
    }
}
