using Moq;
using wrench.auto.repair.cadastro.application.Queries;
using wrench.auto.repair.cadastro.application.Queries.ViewModels;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.application.Events;
using wrench.auto.repair.ordem.servico.application.tests.Fixture;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.application.tests.Events
{
    [Collection(nameof(ClienteCollection))]
    public class OrdemServicoAguardandoAprovacaoEventHandlerTests(ClienteFixture _clienteFixture)
    {
        [Fact(DisplayName = "Event handler não deve falhar quando ordem não existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task Handle_DeveConcluir_QuandoOrdemNaoExistir()
        {
            var id = Guid.NewGuid();
            var mediatorHandler = new Mock<IMediatorHandler>();
            var repo = new Mock<IOrdemServicoRepository>();
            var emailService = new Mock<IEmailService>();
            var emailTemplateRenderer = new Mock<IEmailTemplateRenderer>();

            repo.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrdemServico?)null);

            var handler = new OrdemServicoAguardandoAprovacaoEventHandler(mediatorHandler.Object, repo.Object, emailService.Object, emailTemplateRenderer.Object);
            await handler.Handle(new OrdemServicoAguardandoAprovacaoEvent(id), CancellationToken.None);

            repo.Verify(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Event handler deve consultar ordem quando existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task Handle_DeveConsultarOrdem_QuandoExistir()
        {
            var cliente = _clienteFixture.GerarClienteValido();
            var ordem = new OrdemServico(cliente.Id, Guid.NewGuid(), "Freios", OrdemServicoStatus.AguardandoAprovacao, DateTime.UtcNow);
            var mediatorHandler = new Mock<IMediatorHandler>();
            var repo = new Mock<IOrdemServicoRepository>();
            var emailService = new Mock<IEmailService>();
            var emailTemplateRenderer = new Mock<IEmailTemplateRenderer>();
            var _mapper = _clienteFixture.ConfigurarMapeamentoEGerarMapper();

            var clientViewModel = _mapper.Map<ClienteViewModel>(cliente);

            repo.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);

            mediatorHandler.Setup(m => m.EnviarComando<ObterClientePorIdQuery, ClienteViewModel>(
                    It.Is<ObterClientePorIdQuery>(q => q.ClienteId == ordem.ClienteId)))
                .ReturnsAsync(Result<ClienteViewModel>.Ok(clientViewModel));

            var handler = new OrdemServicoAguardandoAprovacaoEventHandler(mediatorHandler.Object, repo.Object, emailService.Object, emailTemplateRenderer.Object);
            await handler.Handle(new OrdemServicoAguardandoAprovacaoEvent(ordem.Id), CancellationToken.None);

            repo.Verify(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
