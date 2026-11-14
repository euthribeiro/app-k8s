using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.infra.Context;
using wrench.auto.repair.ordem.servico.infra.Repositories;

namespace wrench.auto.repair.ordem.servico.infra.Saga
{
    public class SagaOrdemServicoRepository(OrdemServicoDbContext contexto)
        : Repository<SagaOrdemServico>(contexto), ISagaOrdemServicoRepository
    {
        private static readonly SagaEstado[] EstadosEmEspera =
        [
            SagaEstado.AguardandoOrcamento,
            SagaEstado.AguardandoAprovacao,
            SagaEstado.AguardandoPagamento,
            SagaEstado.AguardandoExecucao,
            SagaEstado.Compensando
        ];

        public Task<SagaOrdemServico?> ObterPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken cancellationToken) =>
            contexto.Set<SagaOrdemServico>()
                .FirstOrDefaultAsync(saga => saga.OrdemServicoId == ordemServicoId, cancellationToken);

        public async Task<IReadOnlyList<SagaOrdemServico>> ObterExpiradasAsync(DateTime agora, int limite, CancellationToken cancellationToken) =>
            await contexto.Set<SagaOrdemServico>()
                .Where(saga => EstadosEmEspera.Contains(saga.Estado)
                    && saga.PrazoEtapa != null
                    && saga.PrazoEtapa <= agora)
                .OrderBy(saga => saga.PrazoEtapa)
                .Take(limite)
                .ToListAsync(cancellationToken);
    }
}
