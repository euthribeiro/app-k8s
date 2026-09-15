using AutoMapper;
using MediatR;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.cadastro.application.Queries
{
    public class VeiculoQueryHandler(
        IMapper _mapper,
        ISortMap<Veiculo> _veiculoSortMap,
        IVeiculoRepository _veiculoRepository
    ) : IRequestHandler<ObterTodosVeiculosQuery, Result<ResultadoPaginado<VeiculoViewModel>>>,
        IRequestHandler<ObterVeiculoPorIdQuery, Result<VeiculoViewModel>>,
        IRequestHandler<ObterVeiculoPorPlacaQuery, Result<VeiculoViewModel>>,
        IRequestHandler<VeiculoExisteEPertenteAoClienteQuery, Result>
    {
        public async Task<Result<ResultadoPaginado<VeiculoViewModel>>> Handle(ObterTodosVeiculosQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ResultadoPaginado<VeiculoViewModel>>.UnprocessableEntity(request.ObterErros());

            var veiculos = await _veiculoRepository
                .BuscaPaginadaAsync(request.Paginacao, _veiculoSortMap.Map, cancellationToken);

            if (veiculos == null)
                return Result<ResultadoPaginado<VeiculoViewModel>>.NotFound("Nenhum veículo cadastrado.");

            var veiculoViewModels = _mapper.Map<ResultadoPaginado<VeiculoViewModel>>(veiculos);

            return Result<ResultadoPaginado<VeiculoViewModel>>.Ok(veiculoViewModels);
        }

        public async Task<Result<VeiculoViewModel>> Handle(ObterVeiculoPorIdQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<VeiculoViewModel>.UnprocessableEntity(request.ObterErros());

            var veiculo = await _veiculoRepository.ObterPorIdAsync(request.VeiculoId, cancellationToken);

            if (veiculo == null)
                return Result<VeiculoViewModel>.NotFound("Veículo não encontrado");

            var veiculoViewModel = _mapper.Map<VeiculoViewModel>(veiculo);

            return Result<VeiculoViewModel>.Ok(veiculoViewModel);
        }

        public async Task<Result<VeiculoViewModel>> Handle(ObterVeiculoPorPlacaQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<VeiculoViewModel>.UnprocessableEntity(request.ObterErros());

            var veiculo = await _veiculoRepository.ObterVeiculoPelaPlacaAsync(request.Placa, cancellationToken);

            if (veiculo == null)
                return Result<VeiculoViewModel>.NotFound("Veículo não encontrado");

            var veiculoViewModel = _mapper.Map<VeiculoViewModel>(veiculo);

            return Result<VeiculoViewModel>.Ok(veiculoViewModel);
        }

        public async Task<Result> Handle(VeiculoExisteEPertenteAoClienteQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var veiculo = await _veiculoRepository.ObterPorIdAsync(request.VeiculoId, cancellationToken);

            if (veiculo == null) return Result.NotFound("Veículo não encontrado");

            if (veiculo.ClienteId != request.ClienteId) return Result.NotFound("Veículo não pertence ao cliente");

            return Result.Ok();
        }
    }
}
