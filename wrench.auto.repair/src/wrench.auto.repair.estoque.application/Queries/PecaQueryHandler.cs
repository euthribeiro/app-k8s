using AutoMapper;
using MediatR;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.estoque.application.Queries.ViewModels;
using wrench.auto.repair.estoque.domain.Data;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.Queries
{
    public class PecaQueryHandler(
        IMapper _mapper,
        ISortMap<Peca> _pecaSortMap,
        IPecaRepository _pecaRepository
    ) : IRequestHandler<ConsultarPecaPorIdQuery, Result<PecaViewModel>>,
        IRequestHandler<ObterTodasPecasQuery, Result<ResultadoPaginado<PecaViewModel>>>
    {
        public async Task<Result<PecaViewModel>> Handle(ConsultarPecaPorIdQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<PecaViewModel>.ValidationError(request.ObterErros());

            var peca = await _pecaRepository.ObterPorIdAsync(request.PecaId, cancellationToken);

            if (peca == null) return Result<PecaViewModel>.NotFound("Peça não encontrada");

            var pecaViewModel = _mapper.Map<PecaViewModel>(peca);

            return Result<PecaViewModel>.Ok(pecaViewModel);
        }

        public async Task<Result<ResultadoPaginado<PecaViewModel>>> Handle(ObterTodasPecasQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ResultadoPaginado<PecaViewModel>>.ValidationError(request.ObterErros());

            var pecas = await _pecaRepository.BuscaPaginadaAsync(request.Paginacao, _pecaSortMap.Map, cancellationToken);

            if (pecas == null)
                return Result<ResultadoPaginado<PecaViewModel>>.NotFound("Peças não encontradas");

            var pecaViewModels = _mapper.Map<ResultadoPaginado<PecaViewModel>>(pecas);

            return Result<ResultadoPaginado<PecaViewModel>>.Ok(pecaViewModels);
        }
    }
}
