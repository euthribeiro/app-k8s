using AutoMapper;
using MediatR;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.cadastro.application.Queries
{
    public class ClienteQueryHandler(
        IMapper _mapper,
        ISortMap<Cliente> _clienteSortMap,
        IClienteRepository _clienteRepository
    ) : IRequestHandler<ObterTodosClientesQuery, Result<ResultadoPaginado<ClienteViewModel>>>,
        IRequestHandler<ObterClientePorIdQuery, Result<ClienteViewModel>>,
        IRequestHandler<ObterClientePorDocumentoQuery, Result<ClienteViewModel>>
    {
        public async Task<Result<ResultadoPaginado<ClienteViewModel>>> Handle(ObterTodosClientesQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ResultadoPaginado<ClienteViewModel>>.UnprocessableEntity(request.ObterErros());

            var clientes = await _clienteRepository
                .BuscaPaginadaAsync(request.Paginacao, _clienteSortMap.Map, cancellationToken);

            if (clientes == null)
                return Result<ResultadoPaginado<ClienteViewModel>>.NotFound("Nenhum cliente cadastrado.");

            var clienteViewModels = _mapper.Map<ResultadoPaginado<ClienteViewModel>>(clientes);

            return Result<ResultadoPaginado<ClienteViewModel>>.Ok(clienteViewModels);
        }

        public async Task<Result<ClienteViewModel>> Handle(ObterClientePorIdQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ClienteViewModel>.UnprocessableEntity(request.ObterErros());

            var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId, cancellationToken);

            if (cliente == null)
                return Result<ClienteViewModel>.NotFound("Cliente não encontrado");

            var clienteViewModel = _mapper.Map<ClienteViewModel>(cliente);

            return Result<ClienteViewModel>.Ok(clienteViewModel);
        }

        public async Task<Result<ClienteViewModel>> Handle(ObterClientePorDocumentoQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<ClienteViewModel>.UnprocessableEntity(request.ObterErros());

            var cliente = await _clienteRepository.ObterPorDocumentAsync(request.Documento, cancellationToken);

            if (cliente == null)
                return Result<ClienteViewModel>.NotFound("Cliente não encontrado");

            var clienteViewModel = _mapper.Map<ClienteViewModel>(cliente);

            return Result<ClienteViewModel>.Ok(clienteViewModel);
        }
    }
}
