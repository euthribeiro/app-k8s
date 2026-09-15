using MediatR;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries.Dtos;
using wrench.auto.repair.ordem.servico.application.Events;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.domain.ValueObjects;

namespace wrench.auto.repair.ordem.servico.application.UseCases.DiagnosticoUseCase
{
    public class DiagnosticoCommandHandler(
        IMediatorHandler _mediatorHandler,
        IOrdemServicoRepository _ordemServicoRepository
    ) : IRequestHandler<SolicitarDiagnosticoCommand, Result>,
        IRequestHandler<RealizarDiagnosticoCommand, Result<Guid>>
    {
        public async Task<Result> Handle(SolicitarDiagnosticoCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
            {
                var erros = request.ValidationResult.Errors.Select(e => e.ErrorMessage);
                return Result.ValidationError(erros);
            }

            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken);

            if (ordemServico == null)
                return Result.NotFound($"Ordem de serviço com ID {request.OrdemServicoId} não encontrada.");

            if (ordemServico.Status != OrdemServicoStatus.Recebida)
                return Result.ValidationError("O estado da ordem de serviço não permite alteração.");

            ordemServico.SolicitaDiagnostico();

            await _ordemServicoRepository.Atualizar(ordemServico);

            var salvo = await _ordemServicoRepository.UnitOfWork.CommitAsync();

            if (!salvo) return Result.Unexpected("Não foi possível solicitar o diagnóstico. Por favor tente novamente.");

            return Result.NoContent();
        }

        public async Task<Result<Guid>> Handle(RealizarDiagnosticoCommand request, CancellationToken cancellationToken)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken);

            if (ordemServico == null)
                return Result<Guid>.NotFound($"Ordem de serviço com ID {request.OrdemServicoId} não encontrada.");

            if (ordemServico.Status != OrdemServicoStatus.EmDiagnostico)
                return Result<Guid>.ValidationError("Ordem de serviço não está em diagnóstico. Inicie o diagnóstico primeiro.");

            ordemServico.AdicionarDiagnostico(request.SolucaoProposta, request.ValorEstimado);

            if (request.PecasId.Count != 0)
            {
                var obterPecasPorIdsCommand = new ObterPecasPorIdsCommand(request.PecasId);

                var resultadoPecas = await _mediatorHandler
                    .ConsultaIntegrada<ObterPecasPorIdsCommand, IEnumerable<PecaDto>>(obterPecasPorIdsCommand);

                if (!resultadoPecas.Sucesso)
                    return Result<Guid>.Custom(resultadoPecas.TipoErro!.Value, resultadoPecas.Erros);

                foreach (var peca in resultadoPecas.Valor!)
                {
                    var item = new ItemOrdemServico(peca.Id, peca.Nome, peca.Valor, peca.Quantidade);
                    ordemServico.AdicionarPeca(item);
                }
            }

            await _ordemServicoRepository.Atualizar(ordemServico);

            var salvo = await _ordemServicoRepository.UnitOfWork.CommitAsync();

            if (!salvo) return Result<Guid>.Unexpected("Não foi possível realizar o diagnóstico. Por favor tente novamente.");

            var ordemServicoAguardandoAprovacaoEvent =
                new OrdemServicoAguardandoAprovacaoEvent(ordemServico.Id);

            await _mediatorHandler.PublicarEvento(ordemServicoAguardandoAprovacaoEvent);

            return Result<Guid>.NoContent();
        }
    }
}
