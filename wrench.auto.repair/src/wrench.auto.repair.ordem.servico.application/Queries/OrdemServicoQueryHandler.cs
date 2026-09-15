using AutoMapper;
using MediatR;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;

namespace wrench.auto.repair.ordem.servico.application.Queries
{
    public class OrdemServicoQueryHandler(
        IMapper _mapper,
        ISortMap<OrdemServico> _ordemServicoSortMap,
        IOrdemServicoRepository _ordemServicoRepository
    ) : IRequestHandler<ObterOrdemServicoIdQuery, Result<OrdemServicoViewModel>>,
        IRequestHandler<ObterTodasOrdemServicoQuery, Result<ResultadoPaginado<OrdemServicoViewModel>>>,
        IRequestHandler<ObterTodasOrdemServicoPorClienteQuery, Result<ResultadoPaginado<OrdemServicoViewModel>>>,
        IRequestHandler<ObterTempoMedioExecucaoOrdemServicoQuery, Result<MonitoramentoViewModel>>
    {
        public async Task<Result<OrdemServicoViewModel>> Handle(ObterOrdemServicoIdQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<OrdemServicoViewModel>.NotFound(request.ObterErros());

            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(request.Id, cancellationToken);

            if (ordemServico == null)
                return Result<OrdemServicoViewModel>.NotFound("Ordem de serviço não encontrada");

            var ordemServicoViewModel = _mapper.Map<OrdemServicoViewModel>(ordemServico);

            return Result<OrdemServicoViewModel>.Ok(ordemServicoViewModel);
        }

        public async Task<Result<ResultadoPaginado<OrdemServicoViewModel>>> Handle(ObterTodasOrdemServicoQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ResultadoPaginado<OrdemServicoViewModel>>.ValidationError(request.ObterErros());

            var ordemServicos = await _ordemServicoRepository
                .BuscaPaginadaAsync(request.Paginacao, _ordemServicoSortMap.Map, cancellationToken);

            if (ordemServicos == null)
                return Result<ResultadoPaginado<OrdemServicoViewModel>>.NotFound("Ordem de serviço não encontradas");

            var ordemServicoViewModels = _mapper.Map<ResultadoPaginado<OrdemServicoViewModel>>(ordemServicos);

            return Result<ResultadoPaginado<OrdemServicoViewModel>>.Ok(ordemServicoViewModels);
        }

        public async Task<Result<ResultadoPaginado<OrdemServicoViewModel>>> Handle(ObterTodasOrdemServicoPorClienteQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ResultadoPaginado<OrdemServicoViewModel>>.ValidationError(request.ObterErros());

            var ordemServicos = await _ordemServicoRepository
                .BuscaPaginadaAsync(request.ClienteId, request.VeiculoId, request.Paginacao, _ordemServicoSortMap.Map, cancellationToken);

            if (ordemServicos == null)
                return Result<ResultadoPaginado<OrdemServicoViewModel>>.NotFound("Ordem de serviço não encontradas");

            var ordemServicoViewModels = _mapper.Map<ResultadoPaginado<OrdemServicoViewModel>>(ordemServicos);

            return Result<ResultadoPaginado<OrdemServicoViewModel>>.Ok(ordemServicoViewModels);
        }

        public async Task<Result<MonitoramentoViewModel>> Handle(ObterTempoMedioExecucaoOrdemServicoQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido()) return Result<MonitoramentoViewModel>.ValidationError(request.ObterErros());

            var tempoMedioMs = await _ordemServicoRepository.ObterTempoMedioExecucaoTodasOrdemServico();

            if (tempoMedioMs == 0) return Result<MonitoramentoViewModel>.NotFound("Nenhum ordem de serviço finalizada para gerar métricas.");

            var tempoMedio = TimeSpan.FromMilliseconds(tempoMedioMs);
            var dias = tempoMedio.Days.ToString().PadLeft(2, '0');
            var horas = tempoMedio.Hours.ToString().PadLeft(2, '0');
            var minutos = tempoMedio.Minutes.ToString().PadLeft(2, '0');
            var segundos = tempoMedio.Seconds.ToString().PadLeft(2, '0');
            var microsegundos = tempoMedio.Microseconds;

            var monitoramentoViewModel = new MonitoramentoViewModel
            {
                TempoMedio = tempoMedio.ToString(),
                TempoMedioMs = tempoMedioMs,
                Descricao = $"O tempo médio de execução de uma ordem de serviço é de {dias} dia(s) {horas} hora(s) {minutos} minuto(s) {segundos} segundo(s) e {microsegundos} microsegundo(s)"
            };

            return Result<MonitoramentoViewModel>.Ok(monitoramentoViewModel);
        }
    }
}
