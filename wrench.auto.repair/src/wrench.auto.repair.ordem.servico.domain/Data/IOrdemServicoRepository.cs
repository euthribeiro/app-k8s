using System.Linq.Expressions;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.ordem.servico.domain.Entities;

namespace wrench.auto.repair.ordem.servico.domain.Data
{
    public interface IOrdemServicoRepository : IRepository<OrdemServico>
    {
        Task<double> ObterTempoMedioExecucaoTodasOrdemServico();
        Task<double> ObterTempoMedioDiagnostico();
        Task<double> ObterTempoMedioExecucao();
        Task<double> ObterTempoMedioEntrega();
        Task<ResultadoPaginado<OrdemServico>> BuscaPaginadaAsync(Guid clienteId, Guid? veiculoId, RequisicaoPaginada request, Dictionary<string, Expression<Func<OrdemServico, object?>>> sortMap, CancellationToken cancellationToken);
    }
}
