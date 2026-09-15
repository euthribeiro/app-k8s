using MediatR;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase
{
    public class OrcamentoCommandHandler(
        IOrdemServicoRepository ordemServicoRepository
    ) : IRequestHandler<AprovaOrcamentoCommand, Result>,
        IRequestHandler<RecusarOrcamentoCommand, Result>
    {
        private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;

        public async Task<Result> Handle(AprovaOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken);

            if (ordemServico == null)
                return Result.NotFound($"Ordem de serviço com ID {request.OrdemServicoId} não encontrada.");

            if (ordemServico.Status != OrdemServicoStatus.AguardandoAprovacao)
                return Result.Conflicted("Status da ordem de serviço não permite aprovação");

            ordemServico.AprovarOrcamento();

            await _ordemServicoRepository.Atualizar(ordemServico);
            var salvo = await _ordemServicoRepository.UnitOfWork.CommitAsync();

            if (!salvo)
                return Result.Unexpected("Não foi possível aprovar o orçamento. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result> Handle(RecusarOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken);

            if (ordemServico == null)
                return Result.NotFound($"Ordem de serviço com identificado {request.OrdemServicoId} não encontrada.");

            if (ordemServico.Status != OrdemServicoStatus.AguardandoAprovacao)
                return Result.Conflicted("Status da ordem de serviço não permite recusa");

            ordemServico.RecusarOrcamento(request.MotivoRecusa);

            await _ordemServicoRepository.Atualizar(ordemServico);
            var salvo = await _ordemServicoRepository.UnitOfWork.CommitAsync();

            if (!salvo)
                return Result.Unexpected("Não foi possível aprovar o orçamento. Por favor tente novamente.");

            return Result.NoContent();
        }
    }
}
