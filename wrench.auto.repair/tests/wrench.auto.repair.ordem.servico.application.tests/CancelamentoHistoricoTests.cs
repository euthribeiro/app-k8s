using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Services;
using wrench.auto.repair.ordem.servico.application.AutoMapper;
using wrench.auto.repair.ordem.servico.application.Events;
using wrench.auto.repair.ordem.servico.application.Queries;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.application.tests
{
    public class CancelamentoHistoricoTests
    {
        private readonly Mock<IOrdemServicoRepository> _repositoryMock = new();
        private readonly Mock<IMediatorHandler> _mediatorMock = new();
        private readonly OrdemServicoCommandHandler _commandHandler;
        private readonly OrdemServicoQueryHandler _queryHandler;

        public CancelamentoHistoricoTests()
        {
            var emailServiceMock = new Mock<IEmailService>();

            _commandHandler = new OrdemServicoCommandHandler(_mediatorMock.Object, _repositoryMock.Object, emailServiceMock.Object);

            var loggerFactory = LoggerFactory.Create(_ => { });
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<OrdemServicoProfile>(), loggerFactory).CreateMapper();
            _queryHandler = new OrdemServicoQueryHandler(mapper, new Mock<ISortMap<OrdemServico>>().Object, _repositoryMock.Object);
        }

        [Fact(DisplayName = "Cancelar ordem deve retornar erro quando comando for inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task Cancelar_DeveRetornarValidationError_QuandoComandoInvalido()
        {
            var result = await _commandHandler.Handle(new CancelarOrdemServicoCommand(Guid.Empty, ""), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Equal(TipoErroEnum.VALIDACAO, result.TipoErro);
            _repositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Cancelar ordem deve retornar not found quando ordem não existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task Cancelar_DeveRetornarNotFound_QuandoOrdemNaoExistir()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

            var result = await _commandHandler.Handle(new CancelarOrdemServicoCommand(id, "Pagamento recusado"), CancellationToken.None);

            Assert.Equal(TipoErroEnum.NAO_ENCONTRADO, result.TipoErro);
        }

        [Fact(DisplayName = "Cancelar ordem deve retornar conflito quando status não permitir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task Cancelar_DeveRetornarConflito_QuandoStatusNaoPermite()
        {
            var ordem = CriarOrdem(OrdemServicoStatus.Finalizada);
            ConfigurarRepositorio(ordem, commit: true);

            var result = await _commandHandler.Handle(new CancelarOrdemServicoCommand(ordem.Id, "Motivo"), CancellationToken.None);

            Assert.Equal(TipoErroEnum.CONFLITO, result.TipoErro);
            Assert.Equal(OrdemServicoStatus.Finalizada, ordem.Status);
            _repositoryMock.Verify(r => r.UnitOfWork.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Cancelar ordem deve cancelar, gravar histórico e notificar o cliente")]
        [Trait("Ordem Serviço", "Application")]
        public async Task Cancelar_DeveCancelarEPublicarEvento()
        {
            var ordem = CriarOrdem(OrdemServicoStatus.EmExecucao);
            ConfigurarRepositorio(ordem, commit: true);

            var result = await _commandHandler.Handle(new CancelarOrdemServicoCommand(ordem.Id, "Execução não aceita a OS"), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(OrdemServicoStatus.Cancelada, ordem.Status);
            Assert.Equal("Execução não aceita a OS", ordem.Historico.Last().Motivo);
            _repositoryMock.Verify(r => r.Atualizar(ordem), Times.Once);
            _mediatorMock.Verify(m => m.PublicarEvento(It.Is<OrdemServicoAtualizadaEvent>(e =>
                e.OrdemServicoId == ordem.Id && e.NovoStatus == OrdemServicoStatus.Cancelada)), Times.Once);
        }

        [Fact(DisplayName = "Cancelar ordem deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task Cancelar_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var ordem = CriarOrdem(OrdemServicoStatus.Recebida);
            ConfigurarRepositorio(ordem, commit: false);

            var result = await _commandHandler.Handle(new CancelarOrdemServicoCommand(ordem.Id, "Motivo"), CancellationToken.None);

            Assert.Equal(TipoErroEnum.INESPERADO, result.TipoErro);
            _mediatorMock.Verify(m => m.PublicarEvento(It.IsAny<OrdemServicoAtualizadaEvent>()), Times.Never);
        }

        [Fact(DisplayName = "Retornar para diagnóstico deve voltar a ordem e registrar a falha")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RetornarParaDiagnostico_DeveVoltarOrdem()
        {
            var ordem = CriarOrdem(OrdemServicoStatus.AguardandoAprovacao);
            ConfigurarRepositorio(ordem, commit: true);

            var result = await _commandHandler.Handle(new RetornarParaDiagnosticoCommand(ordem.Id, "Falha ao gerar orçamento"), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(OrdemServicoStatus.EmDiagnostico, ordem.Status);
            Assert.Equal("Falha ao gerar orçamento", ordem.Historico.Last().Motivo);
        }

        [Fact(DisplayName = "Retornar para diagnóstico deve retornar conflito quando status não permitir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RetornarParaDiagnostico_DeveRetornarConflito_QuandoStatusNaoPermite()
        {
            var ordem = CriarOrdem(OrdemServicoStatus.EmExecucao);
            ConfigurarRepositorio(ordem, commit: true);

            var result = await _commandHandler.Handle(new RetornarParaDiagnosticoCommand(ordem.Id, "Motivo"), CancellationToken.None);

            Assert.Equal(TipoErroEnum.CONFLITO, result.TipoErro);
        }

        [Fact(DisplayName = "Retornar para diagnóstico deve retornar erro quando comando for inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RetornarParaDiagnostico_DeveRetornarValidationError_QuandoComandoInvalido()
        {
            var result = await _commandHandler.Handle(new RetornarParaDiagnosticoCommand(Guid.Empty, ""), CancellationToken.None);

            Assert.Equal(TipoErroEnum.VALIDACAO, result.TipoErro);
        }

        [Fact(DisplayName = "Consultar histórico deve retornar as transições em ordem cronológica")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterHistorico_DeveRetornarTransicoesEmOrdem()
        {
            var ordem = CriarOrdem(OrdemServicoStatus.EmDiagnostico);
            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordem);

            var result = await _queryHandler.Handle(new ObterHistoricoOrdemServicoQuery(ordem.Id), CancellationToken.None);

            Assert.True(result.Sucesso);
            var historico = result.Valor!;
            Assert.Equal(2, historico.Count);
            Assert.Null(historico[0].StatusAnterior);
            Assert.Equal("Recebida", historico[0].StatusNovo);
            Assert.Equal("Recebida", historico[1].StatusAnterior);
            Assert.Equal("Em diagnóstico", historico[1].StatusNovo);
        }

        [Fact(DisplayName = "Consultar histórico deve retornar not found quando ordem não existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterHistorico_DeveRetornarNotFound_QuandoOrdemNaoExistir()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

            var result = await _queryHandler.Handle(new ObterHistoricoOrdemServicoQuery(id), CancellationToken.None);

            Assert.Equal(TipoErroEnum.NAO_ENCONTRADO, result.TipoErro);
        }

        [Fact(DisplayName = "Consultar histórico deve retornar erro quando id for vazio")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterHistorico_DeveRetornarValidationError_QuandoIdVazio()
        {
            var result = await _queryHandler.Handle(new ObterHistoricoOrdemServicoQuery(Guid.Empty), CancellationToken.None);

            Assert.Equal(TipoErroEnum.VALIDACAO, result.TipoErro);
        }

        private void ConfigurarRepositorio(OrdemServico ordem, bool commit)
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync()).ReturnsAsync(commit);
        }

        private static OrdemServico CriarOrdem(OrdemServicoStatus status)
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Revisão", OrdemServicoStatus.Recebida, DateTime.UtcNow);

            if (status == OrdemServicoStatus.Recebida) return ordem;
            ordem.SolicitaDiagnostico();
            if (status == OrdemServicoStatus.EmDiagnostico) return ordem;
            ordem.AdicionarDiagnostico("Pacote revisão completa", 500m);
            if (status == OrdemServicoStatus.AguardandoAprovacao) return ordem;
            ordem.AprovarOrcamento();
            if (status == OrdemServicoStatus.EmExecucao) return ordem;
            ordem.FinalizarOrdemServico();
            return ordem;
        }
    }
}
