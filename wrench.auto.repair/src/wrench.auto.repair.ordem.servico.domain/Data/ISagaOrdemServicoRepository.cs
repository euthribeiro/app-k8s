using wrench.auto.repair.core.Data;
using wrench.auto.repair.ordem.servico.domain.Saga;

namespace wrench.auto.repair.ordem.servico.domain.Data
{
    public interface ISagaOrdemServicoRepository : IRepository<SagaOrdemServico>
    {
        Task<SagaOrdemServico?> ObterPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken cancellationToken);

        Task<IReadOnlyList<SagaOrdemServico>> ObterExpiradasAsync(DateTime agora, int limite, CancellationToken cancellationToken);
    }
}
