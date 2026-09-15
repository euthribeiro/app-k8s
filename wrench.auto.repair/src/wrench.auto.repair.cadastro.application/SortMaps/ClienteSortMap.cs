using System.Linq.Expressions;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.cadastro.application.SortMaps
{
    public class ClienteSortMap : ISortMap<Cliente>
    {
        public Dictionary<string, Expression<Func<Cliente, object?>>> Map => new(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Nome"] = c => c.Nome.Nome,
            ["Documento"] = c => c.Documento.Numeracao,
            ["DataCadastro"] = c => c.DataCadastro
        };
    }
}
