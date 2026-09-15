using AutoMapper;
using MediatR;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries.Dtos;
using wrench.auto.repair.estoque.domain.Data;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.application.Commands
{
    public class PecaCommandHandler(
        IMapper _mapper,
        IPecaRepository _pecaRepository
    ) : IRequestHandler<AtivarPecaCommand, Result>,
        IRequestHandler<InativarPecaCommand, Result>,
        IRequestHandler<CadastrarPecaCommand, Result<Guid>>,
        IRequestHandler<ReporPecaCommand, Result>,
        IRequestHandler<BaixarPecaCommand, Result>,
        IRequestHandler<AtualizarPecaCommand, Result>,
        IRequestHandler<PecaExisteQuery, Result>,
        IRequestHandler<ObterPecasPorIdsCommand, Result<IEnumerable<PecaDto>>>
    {
        public async Task<Result> Handle(AtivarPecaCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var peca = await _pecaRepository.ObterPorIdAsync(request.PecaId, cancellationToken);

            if (peca == null)
                return Result.NotFound("Peça não encontrada");

            if (peca.Ativo) return Result.NoContent();

            peca.Ativar();

            await _pecaRepository.Atualizar(peca);

            var alteracoesSalvas = await _pecaRepository.UnitOfWork.CommitAsync();

            if (!alteracoesSalvas)
                return Result.Unexpected("Não possível ativar a peça. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result> Handle(InativarPecaCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var peca = await _pecaRepository.ObterPorIdAsync(request.PecaId, cancellationToken);

            if (peca == null)
                return Result.NotFound("Peça não encontrada");

            if (!peca.Ativo) return Result.NoContent();

            peca.Inativar();

            await _pecaRepository.Atualizar(peca);

            var alteracoesSalvas = await _pecaRepository.UnitOfWork.CommitAsync();

            if (!alteracoesSalvas)
                return Result.Unexpected("Não possível ativar a peça. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result<Guid>> Handle(CadastrarPecaCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<Guid>.UnprocessableEntity(request.ObterErros());

            var peca = new Peca(request.Nome, request.Descricao, request.Valor,
                                request.Quantidade, request.Ativo, DateTime.UtcNow);

            var pecaExistente = await _pecaRepository
                .ObterPorNomeAsync(peca.Nome, cancellationToken);

            if (pecaExistente != null)
                return Result<Guid>.Conflicted("A peça informada já está cadastrada");

            await _pecaRepository.Adicionar(peca, cancellationToken);

            var pecaSalva = await _pecaRepository.UnitOfWork.CommitAsync();

            if (!pecaSalva)
                return Result<Guid>.Unexpected("Não foi possível cadastrar a peça, por favor tente novamente.");

            return Result<Guid>.Created(peca.Id);
        }

        public async Task<Result> Handle(ReporPecaCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var peca = await _pecaRepository.ObterPorIdAsync(request.PecaId, cancellationToken);

            if (peca == null)
                return Result.NotFound("Peça não encontrada");

            peca.ReporEstoque(request.Quantidade);

            await _pecaRepository.Atualizar(peca);

            var salvo = await _pecaRepository.UnitOfWork.CommitAsync();

            if (!salvo)
                return Result.Unexpected("Não foi possível repor o estoque. Por favor tente novamente.");

            return Result.NoContent();

        }

        public async Task<Result> Handle(BaixarPecaCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var peca = await _pecaRepository.ObterPorIdAsync(request.PecaId, cancellationToken);

            if (peca == null)
                return Result.NotFound("Peça não encontrada");

            peca.BaixarEstoque(request.Quantidade);

            await _pecaRepository.Atualizar(peca);

            var salvo = await _pecaRepository.UnitOfWork.CommitAsync();

            if (!salvo)
                return Result.Unexpected("Não foi possível baixar o estoque. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result> Handle(AtualizarPecaCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.UnprocessableEntity(request.ObterErros());

            var peca = await _pecaRepository.ObterPorIdAsync(request.PecaId, cancellationToken);

            if (peca == null)
                return Result.NotFound("Peça não encontrada");

            if (!peca.Nome.Equals(request.Nome, StringComparison.InvariantCultureIgnoreCase))
            {
                var pecaExistente = await _pecaRepository
                    .ObterPorNomeAsync(peca.Nome, cancellationToken);

                if (pecaExistente != null)
                    return Result.Conflicted("O nome da peça já está sendo utilizado por outro registro.");

                peca.AlterarNome(request.Nome);
            }

            peca.AlterarDescricao(request.Nome);
            peca.AlterarValor(request.Valor);

            if (request.Ativo) peca.Ativar();
            else peca.Inativar();

            await _pecaRepository.Atualizar(peca);

            var salvo = await _pecaRepository.UnitOfWork.CommitAsync();

            if (!salvo) return Result.Unexpected("Não foi possível atualizar a peça. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result> Handle(PecaExisteQuery request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var peca = await _pecaRepository.ObterPorIdAsync(request.PecaId, cancellationToken);

            if (peca == null) Result.NotFound("Peça não encontrada");

            return Result.NoContent();
        }

        public async Task<Result<IEnumerable<PecaDto>>> Handle(ObterPecasPorIdsCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<IEnumerable<PecaDto>>.ValidationError(request.ObterErros());

            var pecas = await _pecaRepository.Buscar(p => request.PecasIds.Contains(p.Id), cancellationToken);

            if (pecas == null) return Result<IEnumerable<PecaDto>>.NotFound("As peças solicitadas não foram encontradas.");

            if (pecas.Count() != request.PecasIds.Count())
                return Result<IEnumerable<PecaDto>>.NotFound("Algumas das peças informadas não foram encontradas");

            var pecasDto = _mapper.Map<IEnumerable<PecaDto>>(pecas);

            return Result<IEnumerable<PecaDto>>.Ok(pecasDto);
        }
    }
}
