using MediatR;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.application.Events;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase
{
    public class OrdemServicoCommandHandler(
        IMediatorHandler _mediatorHandler,
        IOrdemServicoRepository repository,
        IEmailService emailService
    ) : IRequestHandler<CriarOrdemServicoCommand, Result<Guid>>,
        IRequestHandler<FinalizarOrdemServicoCommand, Result>,
        IRequestHandler<EntregarServicoCommand, Result>
    {
        private readonly IOrdemServicoRepository _ordemServicoRepository = repository;
        private readonly IEmailService _emailService = emailService;

        public async Task<Result<Guid>> Handle(CriarOrdemServicoCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<Guid>.ValidationError(request.ObterErros());

            var veiculoExisteEPertenceAoClienteQuery =
                new VeiculoExisteEPertenteAoClienteQuery(request.ClienteId, request.VeiculoId);

            var veiculoExisteEPertenceAoCliente = await _mediatorHandler
                .ConsultaIntegrada(veiculoExisteEPertenceAoClienteQuery);

            if (!veiculoExisteEPertenceAoCliente.Sucesso)
                return Result<Guid>.NotFound("Cliente/Veículo não encontrado");

            OrdemServico ordemServico = request;

            await _ordemServicoRepository.Adicionar(ordemServico, cancellationToken);

            var salvo = await _ordemServicoRepository.UnitOfWork.CommitAsync();

            if (!salvo) return Result<Guid>.Unexpected("Não foi possível criar a ordem de serviço. Por favor tente novamente.");

            var evento = new OrdemServicoAtualizadaEvent(ordemServico.Id, ordemServico.ClienteId, ordemServico.Status);
            await _mediatorHandler.PublicarEvento(evento);

            return Result<Guid>.Created(ordemServico.Id);
        }

        public async Task<Result> Handle(FinalizarOrdemServicoCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken);

            if (ordemServico == null)
                return Result.NotFound("Ordem de serviço não encontrada");

            if (ordemServico.Status != OrdemServicoStatus.EmExecucao)
                return Result.Conflicted("Status da ordem de serviço não permite finalização");

            ordemServico.FinalizarOrdemServico();

            await _ordemServicoRepository.Atualizar(ordemServico);
            var salvo = await _ordemServicoRepository.UnitOfWork.CommitAsync();

            if (!salvo) return Result.Unexpected("Não foi possível atualizar a ordem de serviço. Por favor tente novamente.");

            var evento = new OrdemServicoAtualizadaEvent(ordemServico.Id, ordemServico.ClienteId, ordemServico.Status);
            await _mediatorHandler.PublicarEvento(evento);

            return Result.Ok();
        }

        public async Task<Result> Handle(EntregarServicoCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken);

            if (ordemServico == null)
                return Result.NotFound("Ordem de serviço não encontrada");

            if (ordemServico.Status != OrdemServicoStatus.Finalizada)
                return Result.Conflicted("Finalize a ordem de serviço antes de entregá-la");

            ordemServico.EntregarServico();

            await _ordemServicoRepository.Atualizar(ordemServico);
            var salvo = await _ordemServicoRepository.UnitOfWork.CommitAsync();

            if (!salvo) return Result.Unexpected("Não foi possível atualizar a ordem de serviço. Por favor tente novamente.");

            var evento = new OrdemServicoAtualizadaEvent(ordemServico.Id, ordemServico.ClienteId, ordemServico.Status);
            await _mediatorHandler.PublicarEvento(evento);

            return Result.Ok();
        }

    }
}
