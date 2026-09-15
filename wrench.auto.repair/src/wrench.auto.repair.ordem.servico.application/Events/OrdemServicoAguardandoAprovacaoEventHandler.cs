using MediatR;
using System.Globalization;
using wrench.auto.repair.cadastro.application.Queries;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.domain.Data;

namespace wrench.auto.repair.ordem.servico.application.Events
{
    public class OrdemServicoAguardandoAprovacaoEventHandler(
        IMediatorHandler _mediatorHandler,
        IOrdemServicoRepository _ordemServicoRepository,
        IEmailService _emailService,
        IEmailTemplateRenderer _emailTemplateRenderer
    ) : INotificationHandler<OrdemServicoAguardandoAprovacaoEvent>
    {
        private static readonly CultureInfo _culturaBr = CultureInfo.GetCultureInfo("pt-BR");

        public async Task Handle(OrdemServicoAguardandoAprovacaoEvent notification, CancellationToken cancellationToken)
        {
            var ordemServico = await _ordemServicoRepository
                .ObterPorIdAsync(notification.OrdemServicoId, cancellationToken);

            if (ordemServico == null) return;

            var consulta = new ObterClientePorIdQuery(ordemServico.ClienteId);
            var clienteResult = await _mediatorHandler
                .EnviarComando<ObterClientePorIdQuery, ClienteViewModel>(consulta);

            if (!clienteResult.Sucesso || clienteResult.Valor == null) return;

            var cliente = clienteResult.Valor;

            var corpo = _emailTemplateRenderer.Render(
                "OrdemServicoAtualizacao",
                new Dictionary<string, string?>
                {
                    ["ClienteNome"] = cliente.Nome,
                    ["Status"] = "aguardando a sua aprovação",
                    ["OrdemServicoId"] = ordemServico.Id.ToString(),
                    ["Descricao"] = ordemServico.Descricao,
                    ["SolucaoProposta"] = ordemServico.SolucaoProposta,
                    ["ValorServico"] = ordemServico.ValorServico.ToString("C", _culturaBr),
                    ["LinkAcompanhamento"] = $"http://api.bgt3.com.br/api/v1/ordem-servico/{ordemServico.Id}",
                    ["Ano"] = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture)
                });

            var mensagem = new EmailMessage
            {
                To = [cliente.Email],
                Subject = $"Ordem de Serviço {ordemServico.Id}",
                Body = corpo,
                IsHtml = true
            };

            try
            {
                await _emailService.EnviarAsync(mensagem, cancellationToken);
            }
            catch (Exception) { }
        }
    }
}
