using System.Linq.Expressions;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;

namespace wrench.auto.repair.ordem.servico.application.SortMaps
{
    public class OrdemServicoSortMap : ISortMap<OrdemServico>
    {
        public Dictionary<string, Expression<Func<OrdemServico, object?>>> Map => new(StringComparer.InvariantCultureIgnoreCase)
        {
            ["DataCriacao"] = c => c.DataCriacao,
            ["Status"] = c => c.Status,
            ["ValorServico"] = c => c.ValorServico,
            ["StatusAprovacao"] = c => c.StatusAprovacao,
            ["DataDiagnostico"] = c => c.DataDiagnostico,
            ["DataEnvio"] = c => c.DataEnvio,
            ["DataAprovacaoRecusa"] = c => c.DataAprovacaoRecusa,
            ["DataEntrega"] = c => c.DataEntrega
        };
    }
}
