using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.infra.Context;
using wrench.auto.repair.ordem.servico.infra.Extensions;

namespace wrench.auto.repair.ordem.servico.infra.Repositories
{
    public class OrdemServicoRepository(OrdemServicoDbContext _context) : Repository<OrdemServico>(_context), IOrdemServicoRepository
    {
        public async Task<ResultadoPaginado<OrdemServico>> BuscaPaginadaAsync(Guid clienteId, Guid? veiculoId, RequisicaoPaginada request, Dictionary<string, Expression<Func<OrdemServico, object?>>> sortMap, CancellationToken cancellationToken)
        {
            var query = _context.Set<OrdemServico>().AsQueryable()
                .Where(c => c.ClienteId == clienteId);

            if (veiculoId.HasValue && veiculoId != Guid.Empty)
                query = query.Where(o => o.VeiculoId == veiculoId);

            ValidarOrdenacao(request, sortMap);

            if (!string.IsNullOrWhiteSpace(request.OrdenarPor)
                && sortMap.TryGetValue(request.OrdenarPor, out var orderExpression))
            {
                query = request.Decrescente
                    ? query.OrderByDescending(orderExpression)
                    : query.OrderBy(orderExpression);

                if (!request.OrdenarPor.Equals("DataCriacao", StringComparison.InvariantCultureIgnoreCase))
                    query = query.OrderByDescending(c => c.DataCriacao);
            }
            else
            {
                query = query.OrderByDescending(c => c.DataCriacao);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.NumeroPagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return new ResultadoPaginado<OrdemServico>(
                items,
                totalCount,
                request.NumeroPagina,
                request.TamanhoPagina,
                sortMap.Keys);
        }

        public async Task<ResultadoPaginado<OrdemServico>> BuscaPaginadaAsync(
            RequisicaoPaginada request,
            Dictionary<string, Expression<Func<OrdemServico, object>>> sortMap,
            CancellationToken cancellationToken)
        {
            var query = _context.Set<OrdemServico>().AsQueryable()
                .Where(os => os.Status != OrdemServicoStatus.Finalizada && os.Status != OrdemServicoStatus.Entregue);

            ValidarOrdenacao(request, sortMap);

            if (!string.IsNullOrWhiteSpace(request.OrdenarPor)
                && sortMap.TryGetValue(request.OrdenarPor, out var orderExpression))
            {
                query = request.Decrescente
                    ? query.OrderByDescending(orderExpression)
                    : query.OrderBy(orderExpression);


                if (!request.OrdenarPor.Equals("DataCriacao", StringComparison.InvariantCultureIgnoreCase))
                    query = query.OrderByDescending(c => c.DataCriacao);
            }
            else
            {
                query = query.OrderByDescending(c => c.DataCriacao);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.NumeroPagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return new ResultadoPaginado<OrdemServico>(
                items,
                totalCount,
                request.NumeroPagina,
                request.TamanhoPagina,
                sortMap.Keys);
        }

        public async Task<double> ObterTempoMedioExecucaoTodasOrdemServico()
        {
            var result = await _context.OrdemServico
                .Where(o =>
                    (o.DataAprovacaoRecusa.HasValue) &&
                    (o.DataFinalizacao.HasValue || o.DataEntrega.HasValue) &&
                    (o.Status == OrdemServicoStatus.Finalizada ||
                     o.Status == OrdemServicoStatus.Entregue))
                .AverageAsync(o =>
                    (double?)PostgresDbFunctions.DateDiffMilliseconds(
                        o.DataAprovacaoRecusa!.Value,
                        (o.DataFinalizacao ?? o.DataEntrega!.Value)
                    )
                );

            return result ?? 0;
        }

        /// <summary>
        /// Tempo médio, em milissegundos, da criação da ordem de serviço até a conclusão do diagnóstico.
        /// Considera apenas ordens com <c>DataEnvio</c>, que só é preenchida ao concluir o diagnóstico e não
        /// é sobrescrita depois. O intervalo inclui o tempo no status <c>Recebida</c>, que não tem data própria.
        /// </summary>
        public async Task<double> ObterTempoMedioDiagnostico()
        {
            var result = await _context.OrdemServico
                .Where(o => o.DataEnvio.HasValue)
                .AverageAsync(o =>
                    (double?)PostgresDbFunctions.DateDiffMilliseconds(
                        o.DataCriacao,
                        o.DataDiagnostico!.Value
                    )
                );

            return result ?? 0;
        }

        /// <summary>
        /// Tempo médio, em milissegundos, entre a aprovação do orçamento e a finalização da ordem de serviço.
        /// <c>DataFinalizacao</c> só existe no caminho aprovado, porque a finalização exige o status
        /// <c>EmExecucao</c>; ordens recusadas ficam fora sem filtro adicional.
        /// </summary>
        public async Task<double> ObterTempoMedioExecucao()
        {
            var result = await _context.OrdemServico
                .Where(o => o.DataFinalizacao.HasValue)
                .AverageAsync(o =>
                    (double?)PostgresDbFunctions.DateDiffMilliseconds(
                        o.DataAprovacaoRecusa!.Value,
                        o.DataFinalizacao!.Value
                    )
                );

            return result ?? 0;
        }

        public async Task<double> ObterTempoMedioEntrega()
        {
            var result = await _context.OrdemServico
                .Where(o => o.DataEntrega.HasValue && o.DataFinalizacao.HasValue)
                .AverageAsync(o =>
                    (double?)PostgresDbFunctions.DateDiffMilliseconds(
                        o.DataFinalizacao!.Value,
                        o.DataEntrega!.Value
                    )
                );

            return result ?? 0;
        }
    }
}

