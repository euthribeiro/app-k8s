using Moq;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries.Dtos;
using wrench.auto.repair.ordem.servico.application.Events;
using wrench.auto.repair.ordem.servico.application.UseCases.DiagnosticoUseCase;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.application.tests
{
    public class DiagnosticoTests
    {
        private readonly Mock<IOrdemServicoRepository> _ordemServicoRepositoryMock;
        private readonly Mock<IMediatorHandler> _mediatorHandlerMock;
        private readonly DiagnosticoCommandHandler _handler;

        public DiagnosticoTests()
        {
            _ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
            _mediatorHandlerMock = new Mock<IMediatorHandler>();
            _mediatorHandlerMock
                .Setup(m => m.PublicarEvento(It.IsAny<OrdemServicoAguardandoAprovacaoEvent>()))
                .Returns(Task.CompletedTask);
            _handler = new DiagnosticoCommandHandler(_mediatorHandlerMock.Object, _ordemServicoRepositoryMock.Object);
        }

        [Fact(DisplayName = "Realizar diagnóstico command deve ser inválido para dados incorretos")]
        [Trait("Ordem Serviço", "Application")]
        public void RealizarDiagnosticoCommand_DeveSerInvalido_QuandoDadosIncorretos()
        {
            var command = new RealizarDiagnosticoCommand(Guid.Empty, 0m, string.Empty, []);

            var valido = command.EhValido();

            Assert.False(valido);
        }

        [Fact(DisplayName = "Solicitar Diagnóstico Com Sucesso")]
        [Trait("Ordem Serviço", "Application")]
        public async Task SolicitarDiagnostico_OrdemServico_Valida_Com_Sucesso()
        {
            var ordemServicoId = Guid.NewGuid();
            var command = new SolicitarDiagnosticoCommand(ordemServicoId);
            var ordemServicoFake = new OrdemServico(Guid.NewGuid(), ordemServicoId, "Barulho na roda", OrdemServicoStatus.Recebida, DateTime.Now);

            _ordemServicoRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(ordemServicoId, CancellationToken.None))
                .ReturnsAsync(ordemServicoFake);

            _ordemServicoRepositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .Returns(Task.FromResult(true));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            _ordemServicoRepositoryMock.Verify(repo => repo.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Solicitar Diagnóstico Ordem Serviço Inexistente")]
        [Trait("Ordem Serviço", "Application")]
        public async Task SolicitarDiagnostico_OrdemServico_Nao_Existente_Retorna_Erro()
        {
            var ordemServicoId = Guid.NewGuid();
            var command = new SolicitarDiagnosticoCommand(Guid.NewGuid());
            var ordemServicoFake = new OrdemServico(Guid.NewGuid(), ordemServicoId, "Barulho na roda", OrdemServicoStatus.Recebida, DateTime.Now);

            _ordemServicoRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(ordemServicoId, CancellationToken.None))
                .ReturnsAsync(ordemServicoFake);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            _ordemServicoRepositoryMock.Verify(repo => repo.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Realizar Diagnóstico Dados Válido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RealizarDiagnostico_Deve_Realizar_Diagnostico_Com_Sucesso_Quando_Dados_Forem_Validos()
        {
            var ordemServicoId = Guid.NewGuid();
            var pecaId = Guid.NewGuid();
            var pecasIds = new HashSet<Guid> { pecaId };

            var command = new RealizarDiagnosticoCommand(
                ordemServicoId,
                200.00m,
                "Substituir pastilhas de freio",
                pecasIds
            );

            var pecasRetorno = new List<PecaDto>
            {
                new()
                {
                    Id = pecaId,
                    Nome = "Pastilha de freio dianteira",
                    Valor = 89.90m,
                    Quantidade = 2
                }
            };

            _mediatorHandlerMock
                .Setup(m => m.ConsultaIntegrada<ObterPecasPorIdsCommand, IEnumerable<PecaDto>>(It.IsAny<ObterPecasPorIdsCommand>()))
                .ReturnsAsync(Result<IEnumerable<PecaDto>>.Ok(pecasRetorno));

            var ordemServicoFake = new OrdemServico(Guid.NewGuid(), ordemServicoId, "Barulho na roda", OrdemServicoStatus.EmDiagnostico, DateTime.Now);
            _ordemServicoRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(ordemServicoId, CancellationToken.None))
                .ReturnsAsync(ordemServicoFake);
            _ordemServicoRepositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);

            _ordemServicoRepositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                  .Returns(Task.FromResult(true));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            _ordemServicoRepositoryMock.Verify(repo => repo.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorHandlerMock.Verify(
                m => m.ConsultaIntegrada<ObterPecasPorIdsCommand, IEnumerable<PecaDto>>(
                    It.Is<ObterPecasPorIdsCommand>(q => q.PecasIds.Contains(pecaId))),
                Times.Once);
            var item = Assert.Single(ordemServicoFake.Pecas);
            Assert.Equal(pecaId, item.PecaId);
            Assert.Equal("Pastilha de freio dianteira", item.Nome);
            Assert.Equal(89.90m, item.ValorUnitario);
            Assert.Equal(2, item.Quantidade);
        }

        [Fact(DisplayName = "Realizar Diagnóstico Ordem Serviço Inexistente")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RealizarDiagnostico_Deve_Retornar_Erro_Quando_Ordem_De_Servico_Nao_Existir()
        {
            var ordemServicoId = Guid.NewGuid();

            var command = new RealizarDiagnosticoCommand(
                ordemServicoId,
                200.00m,
                "Substituir pastilhas de freio",
                []
            );

            var ordemServicoFake = new OrdemServico(Guid.NewGuid(), ordemServicoId, "Barulho na roda", OrdemServicoStatus.EmDiagnostico, DateTime.Now);

            _ordemServicoRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(Guid.NewGuid(), CancellationToken.None))
                .ReturnsAsync(ordemServicoFake);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            _ordemServicoRepositoryMock.Verify(repo => repo.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Solicitar diagnóstico deve retornar validação quando comando for inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task SolicitarDiagnostico_DeveRetornarValidationError_QuandoCommandInvalido()
        {
            var command = new SolicitarDiagnosticoCommand(Guid.Empty);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            _ordemServicoRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Solicitar diagnóstico deve retornar erro quando status for inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task SolicitarDiagnostico_DeveRetornarValidationError_QuandoStatusInvalido()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Suspensão", OrdemServicoStatus.EmExecucao, DateTime.UtcNow);
            var command = new SolicitarDiagnosticoCommand(ordem.Id);

            _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Solicitar diagnóstico deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task SolicitarDiagnostico_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Injeção", OrdemServicoStatus.Recebida, DateTime.UtcNow);
            var command = new SolicitarDiagnosticoCommand(ordem.Id);

            _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _ordemServicoRepositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _ordemServicoRepositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Realizar diagnóstico deve retornar validação quando ordem estiver em status inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RealizarDiagnostico_DeveRetornarValidationError_QuandoStatusInvalido()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Direção", OrdemServicoStatus.Recebida, DateTime.UtcNow);
            var command = new RealizarDiagnosticoCommand(ordem.Id, 120m, "Ajustar caixa de direção", []);

            _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Realizar diagnóstico deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RealizarDiagnostico_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Bateria", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            var command = new RealizarDiagnosticoCommand(ordem.Id, 350m, "Troca de bateria", []);

            _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _ordemServicoRepositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _ordemServicoRepositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Realizar diagnóstico deve retornar erro quando consulta de peças falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RealizarDiagnostico_DeveRetornarErro_QuandoConsultaPecasFalhar()
        {
            var pecaId = Guid.NewGuid();
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Elétrica", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            var command = new RealizarDiagnosticoCommand(ordem.Id, 90m, "Reparo chicote", [pecaId]);

            _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);

            _mediatorHandlerMock
                .Setup(m => m.ConsultaIntegrada<ObterPecasPorIdsCommand, IEnumerable<PecaDto>>(It.IsAny<ObterPecasPorIdsCommand>()))
                .ReturnsAsync(Result<IEnumerable<PecaDto>>.NotFound("Peças não encontradas"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            _ordemServicoRepositoryMock.Verify(r => r.UnitOfWork.CommitAsync(), Times.Never);
        }
    }
}
