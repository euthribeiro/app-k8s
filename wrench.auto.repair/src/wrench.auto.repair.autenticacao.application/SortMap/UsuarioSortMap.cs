using System.Linq.Expressions;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.autenticacao.application.SortMap
{
    public class UsuarioSortMap : ISortMap<Usuario>
    {
        public Dictionary<string, Expression<Func<Usuario, object?>>> Map => new(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Perfil"] = c => c.Perfil.Nome,
            ["DataCadastro"] = c => c.DataCadastro
        };
    }
}
