using System.Linq.Expressions;
using AutoMapper;
using Moq;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Messages.CommonMessages.IntegratedQueries;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.ordem.servico.application.Paginacao;
using wrench.auto.repair.ordem.servico.application.Queries;
using wrench.auto.repair.ordem.servico.application.Queries.ViewModels;
using wrench.auto.repair.ordem.servico.application.UseCases.OrcamentoUseCase;
using wrench.auto.repair.ordem.servico.application.UseCases.OrdemServicoUseCase;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.application.tests
{
    public class OrdemServicoTests
    {
        private readonly Mock<IOrdemServicoRepository> _repositoryMock;
        private readonly Mock<IMediatorHandler> _mediatorMock;
        private readonly OrdemServicoCommandHandler _commandHandler;
        private readonly OrcamentoCommandHandler _orcamentoHandler;
        private readonly OrdemServicoQueryHandler _queryHandler;

        public OrdemServicoTests()
        {
            _repositoryMock = new Mock<IOrdemServicoRepository>();
            _mediatorMock = new Mock<IMediatorHandler>();
            var mapper = new Mock<IMapper>();
            var ordemServicoSortMap = new Mock<ISortMap<OrdemServico>>();
            mapper.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<OrdemServico>()))
                .Returns((OrdemServico o) => new OrdemServicoViewModel
                {
                    Id = o.Id,
                    ClienteId = o.ClienteId,
                    VeiculoId = o.VeiculoId,
                    Descricao = o.Descricao,
                    DataCriacao = o.DataCriacao,
                    Status = o.Status.ToString(),
                    ValorServico = o.ValorServico,
                    SolucaoProposta = o.SolucaoProposta ?? string.Empty,
                    DataDiagnostico = o.DataDiagnostico,
                    DataEnvio = o.DataEnvio,
                    StatusAprovacao = o.StatusAprovacao.ToString(),
                    DataAprovacaoRecusa = o.DataAprovacaoRecusa,
                    ValorTotal = o.CalcularValorTotal()
                });

            mapper.Setup(m => m.Map<ResultadoPaginado<OrdemServicoViewModel>>(It.IsAny<ResultadoPaginado<OrdemServico>>()))
                .Returns((ResultadoPaginado<OrdemServico> src) =>
                {
                    var vms = src.Itens.Select(o => new OrdemServicoViewModel
                    {
                        Id = o.Id,
                        ClienteId = o.ClienteId,
                        VeiculoId = o.VeiculoId,
                        Descricao = o.Descricao,
                        DataCriacao = o.DataCriacao,
                        Status = o.Status.ToString(),
                        ValorServico = o.ValorServico,
                        SolucaoProposta = o.SolucaoProposta ?? string.Empty,
                        DataDiagnostico = o.DataDiagnostico,
                        DataEnvio = o.DataEnvio,
                        StatusAprovacao = o.StatusAprovacao.ToString(),
                        DataAprovacaoRecusa = o.DataAprovacaoRecusa,
                        DataEntrega = o.DataEntrega,
                        Pecas = [],
                        ValorTotal = o.CalcularValorTotal()
                    }).ToList();
                    return new ResultadoPaginado<OrdemServicoViewModel>(vms, src.TotalRegistros, src.NumeroPagina, src.TamanhoPagina, src.OrdenacoesPermitidas);
                });

            var emailServiceMock = new Mock<wrench.auto.repair.core.Services.IEmailService>();
            emailServiceMock.Setup(e => e.EnviarAsync(It.IsAny<wrench.auto.repair.core.Services.EmailMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wrench.auto.repair.core.Errors.Result<string>.Ok(string.Empty));

            _commandHandler = new OrdemServicoCommandHandler(_mediatorMock.Object, _repositoryMock.Object, emailServiceMock.Object);
            _orcamentoHandler = new OrcamentoCommandHandler(_repositoryMock.Object);
            _queryHandler = new OrdemServicoQueryHandler(mapper.Object, ordemServicoSortMap.Object, _repositoryMock.Object);
        }

        [Fact(DisplayName = "Criar Ordem Serviço Com Sucesso")]
        [Trait("Ordem Serviço", "Application")]
        public async Task CriarOrdemServico_DeveCriarEAdicionarOrdemServico_QuandoCommandValido()
        {
            var command = new CriarOrdemServicoCommand(Guid.NewGuid(), Guid.NewGuid(), "Cliente falou que o carro tá com barulho na roda.");

            _mediatorMock.Setup(m => m.ConsultaIntegrada(It.IsAny<VeiculoExisteEPertenteAoClienteQuery>()))
                         .ReturnsAsync(Result.Ok());

            _repositoryMock.Setup(r => r.Adicionar(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                         .Returns(Task.FromResult(true));

            var resultado = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.NotNull(resultado);
            Assert.True(resultado.Sucesso);
            _repositoryMock.Verify(r => r.Adicionar(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Criar Ordem Serviço Comando Inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task CriarOrdemServico_DeveRetornarErro_QuandoDadosForemInvalidos()
        {
            var command = new CriarOrdemServicoCommand(default, default, String.Empty);

            var resultado = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(resultado.Sucesso);
        }

        [Fact(DisplayName = "Consultar Ordem Serviço Com Sucesso")]
        [Trait("Ordem Serviço", "Application")]
        public async Task OrdemServico_DeveConsultarPorId_QuandoQueryValido()
        {
            var id = Guid.NewGuid();
            var query = new ObterOrdemServicoIdQuery(id);
            var ordemServico = new OrdemServico(id, Guid.NewGuid(), "Veículo Teste", OrdemServicoStatus.Recebida, DateTime.UtcNow);
            ordemServico.Id = id;

            _repositoryMock.Setup(r => r.ObterPorIdAsync(id, CancellationToken.None))
                .ReturnsAsync(ordemServico);

            var result = await _queryHandler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(id, result.Valor!.Id);
            _repositoryMock.Verify(r => r.ObterPorIdAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact(DisplayName = "Consultar Ordem Serviço Com Falha - Não Encontrado")]
        [Trait("Ordem Serviço", "Application")]
        public async Task OrdemServico_DeveRetornarNulo_QuandoOrdemServicoNaoExistir()
        {
            var id = Guid.NewGuid();
            var query = new ObterOrdemServicoIdQuery(id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(id, CancellationToken.None))
                .ReturnsAsync((OrdemServico?)null);

            var result = await _queryHandler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            _repositoryMock.Verify(r => r.ObterPorIdAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact(DisplayName = "Finalizar ordem deve retornar erro quando comando for inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task FinalizarOrdemServico_DeveRetornarValidationError_QuandoCommandInvalido()
        {
            var command = new FinalizarOrdemServicoCommand(Guid.Empty);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            _repositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Finalizar ordem deve retornar not found quando ordem não existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task FinalizarOrdemServico_DeveRetornarNotFound_QuandoOrdemNaoExistir()
        {
            var ordemId = Guid.NewGuid();
            var command = new FinalizarOrdemServicoCommand(ordemId);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrdemServico?)null);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Finalizar ordem deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task FinalizarOrdemServico_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Troca de pneu", OrdemServicoStatus.EmExecucao, DateTime.UtcNow);
            var command = new FinalizarOrdemServicoCommand(ordem.Id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Finalizar ordem deve finalizar com sucesso quando ordem estiver em execução")]
        [Trait("Ordem Serviço", "Application")]
        public async Task FinalizarOrdemServico_DeveFinalizar_QuandoStatusEmExecucao()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Alinhamento", OrdemServicoStatus.EmExecucao, DateTime.UtcNow);
            var command = new FinalizarOrdemServicoCommand(ordem.Id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(true);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(OrdemServicoStatus.Finalizada, ordem.Status);
        }

        [Fact(DisplayName = "Aprovar orçamento deve retornar not found quando ordem não existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task AprovarOrcamento_DeveRetornarNotFound_QuandoOrdemNaoExistir()
        {
            var ordemId = Guid.NewGuid();
            var command = new AprovaOrcamentoCommand(ordemId);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrdemServico?)null);

            var result = await _orcamentoHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Aprovar orçamento deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task AprovarOrcamento_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Freio", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            ordem.AdicionarDiagnostico("Troca de disco e pastilha", 780m);
            var command = new AprovaOrcamentoCommand(ordem.Id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var result = await _orcamentoHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Aprovar orçamento deve atualizar status para em execução")]
        [Trait("Ordem Serviço", "Application")]
        public async Task AprovarOrcamento_DeveAtualizarStatus_QuandoSucesso()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Motor", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            ordem.AdicionarDiagnostico("Troca de bomba d'água", 1200m);
            var command = new AprovaOrcamentoCommand(ordem.Id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(true);

            var result = await _orcamentoHandler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(OrdemServicoStatus.EmExecucao, ordem.Status);
        }

        [Fact(DisplayName = "Criar ordem deve retornar not found quando veículo não pertence ao cliente")]
        [Trait("Ordem Serviço", "Application")]
        public async Task CriarOrdemServico_DeveRetornarNotFound_QuandoVeiculoNaoPertenceAoCliente()
        {
            var command = new CriarOrdemServicoCommand(Guid.NewGuid(), Guid.NewGuid(), "Ruído no motor");

            _mediatorMock.Setup(m => m.ConsultaIntegrada(It.IsAny<VeiculoExisteEPertenteAoClienteQuery>()))
                .ReturnsAsync(Result.NotFound());

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Criar ordem deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task CriarOrdemServico_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var command = new CriarOrdemServicoCommand(Guid.NewGuid(), Guid.NewGuid(), "Ruído no câmbio");

            _mediatorMock.Setup(m => m.ConsultaIntegrada(It.IsAny<VeiculoExisteEPertenteAoClienteQuery>()))
                .ReturnsAsync(Result.Ok());
            _repositoryMock.Setup(r => r.Adicionar(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Query deve retornar erro quando identificador for inválido")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterOrdemServicoIdQuery_DeveRetornarErro_QuandoIdInvalido()
        {
            var query = new ObterOrdemServicoIdQuery(Guid.Empty);

            var result = await _queryHandler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            _repositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Aprova orçamento command deve ser inválido quando ordem não informada")]
        [Trait("Ordem Serviço", "Application")]
        public void AprovaOrcamentoCommand_DeveSerInvalido_QuandoIdVazio()
        {
            var command = new AprovaOrcamentoCommand(Guid.Empty);

            var valido = command.EhValido();

            Assert.False(valido);
        }

        [Fact(DisplayName = "Finalizar ordem command deve ser inválido quando ordem não informada")]
        [Trait("Ordem Serviço", "Application")]
        public void FinalizarOrdemServicoCommand_DeveSerInvalido_QuandoIdVazio()
        {
            var command = new FinalizarOrdemServicoCommand(Guid.Empty);

            var valido = command.EhValido();

            Assert.False(valido);
        }

        [Fact(DisplayName = "Recusar orçamento deve concluir com sucesso")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RecusarOrcamento_DeveRecusar_QuandoOrdemExistir()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Ar condicionado", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            ordem.AdicionarDiagnostico("Recarga de gás", 400m);
            var command = new RecusarOrcamentoCommand(ordem.Id, "Cliente desistiu do serviço");

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(true);

            var result = await _orcamentoHandler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(OrdemServicoStatus.Finalizada, ordem.Status);
        }

        [Fact(DisplayName = "Recusar orçamento deve retornar not found quando ordem não existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RecusarOrcamento_DeveRetornarNotFound_QuandoOrdemNaoExistir()
        {
            var ordemId = Guid.NewGuid();
            var command = new RecusarOrcamentoCommand(ordemId, "Motivo");

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrdemServico?)null);

            var result = await _orcamentoHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Recusar orçamento deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task RecusarOrcamento_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Embreagem", OrdemServicoStatus.EmDiagnostico, DateTime.UtcNow);
            ordem.AdicionarDiagnostico("Troca de kit", 2100m);
            var command = new RecusarOrcamentoCommand(ordem.Id, "Orçamento acima do esperado");

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var result = await _orcamentoHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Entregar serviço deve retornar erro de validação quando id for vazio")]
        [Trait("Ordem Serviço", "Application")]
        public async Task EntregarServico_DeveRetornarValidationError_QuandoIdInvalido()
        {
            var command = new EntregarServicoCommand(Guid.Empty);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            _repositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Entregar serviço deve retornar not found quando ordem não existir")]
        [Trait("Ordem Serviço", "Application")]
        public async Task EntregarServico_DeveRetornarNotFound_QuandoOrdemNaoExistir()
        {
            var ordemId = Guid.NewGuid();
            var command = new EntregarServicoCommand(ordemId);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrdemServico?)null);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Entregar serviço deve retornar forbidden quando ordem não estiver finalizada")]
        [Trait("Ordem Serviço", "Application")]
        public async Task EntregarServico_DeveRetornarForbidden_QuandoStatusNaoFinalizada()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", OrdemServicoStatus.EmExecucao, DateTime.UtcNow);
            var command = new EntregarServicoCommand(ordem.Id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            _repositoryMock.Verify(r => r.Atualizar(It.IsAny<OrdemServico>()), Times.Never);
        }

        [Fact(DisplayName = "Entregar serviço deve retornar erro inesperado quando commit falhar")]
        [Trait("Ordem Serviço", "Application")]
        public async Task EntregarServico_DeveRetornarUnexpected_QuandoCommitFalhar()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", OrdemServicoStatus.Finalizada, DateTime.UtcNow);
            var command = new EntregarServicoCommand(ordem.Id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(false);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Entregar serviço deve concluir quando ordem estiver finalizada")]
        [Trait("Ordem Serviço", "Application")]
        public async Task EntregarServico_DeveEntregar_QuandoFinalizada()
        {
            var ordem = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", OrdemServicoStatus.Finalizada, DateTime.UtcNow);
            var command = new EntregarServicoCommand(ordem.Id);

            _repositoryMock.Setup(r => r.ObterPorIdAsync(ordem.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordem);
            _repositoryMock.Setup(r => r.Atualizar(It.IsAny<OrdemServico>()))
                .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.UnitOfWork.CommitAsync())
                .ReturnsAsync(true);

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(OrdemServicoStatus.Entregue, ordem.Status);
            Assert.NotNull(ordem.DataEntrega);
        }

        [Fact(DisplayName = "Obter ordens por cliente deve retornar erro quando clienteId for vazio")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterTodasOrdemServicoPorCliente_DeveRetornarValidationError_QuandoClienteInvalido()
        {
            var paginacao = new OrdemServicoRequisicaoPaginada();
            var query = new ObterTodasOrdemServicoPorClienteQuery(Guid.Empty, null, paginacao);

            var result = await _queryHandler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            _repositoryMock.Verify(r => r.BuscaPaginadaAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<OrdemServicoRequisicaoPaginada>(),
                It.IsAny<Dictionary<string, Expression<Func<OrdemServico, object?>>>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Obter ordens por cliente deve retornar erro quando ordenação for inválida")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterTodasOrdemServicoPorCliente_DeveRetornarValidationError_QuandoOrdenacaoInvalida()
        {
            var paginacao = new OrdemServicoRequisicaoPaginada { OrdenarPor = "ColunaNaoPermitida" };
            var query = new ObterTodasOrdemServicoPorClienteQuery(Guid.NewGuid(), null, paginacao);

            var result = await _queryHandler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            _repositoryMock.Verify(r => r.BuscaPaginadaAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<OrdemServicoRequisicaoPaginada>(),
                It.IsAny<Dictionary<string, Expression<Func<OrdemServico, object?>>>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Obter ordens por cliente deve retornar not found quando repositório não retornar dados")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterTodasOrdemServicoPorCliente_DeveRetornarNotFound_QuandoRepositorioNulo()
        {
            var clienteId = Guid.NewGuid();
            var paginacao = new OrdemServicoRequisicaoPaginada();
            var query = new ObterTodasOrdemServicoPorClienteQuery(clienteId, null, paginacao);

            _repositoryMock.Setup(r => r.BuscaPaginadaAsync(
                    clienteId,
                    null,
                    paginacao,
                    It.IsAny<Dictionary<string, Expression<Func<OrdemServico, object?>>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResultadoPaginado<OrdemServico>)null!);

            var result = await _queryHandler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "Obter ordens por cliente deve retornar lista paginada")]
        [Trait("Ordem Serviço", "Application")]
        public async Task ObterTodasOrdemServicoPorCliente_DeveRetornarOk_QuandoExistiremOrdens()
        {
            var clienteId = Guid.NewGuid();
            var veiculoId = Guid.NewGuid();
            var paginacao = new OrdemServicoRequisicaoPaginada();
            var ordem = new OrdemServico(clienteId, veiculoId, "Teste", OrdemServicoStatus.Recebida, DateTime.UtcNow);
            var resultadoPaginado = new ResultadoPaginado<OrdemServico>([ordem], 1, 1, 10);

            var query = new ObterTodasOrdemServicoPorClienteQuery(clienteId, veiculoId, paginacao);

            _repositoryMock.Setup(r => r.BuscaPaginadaAsync(
                    clienteId,
                    veiculoId,
                    paginacao,
                    It.IsAny<Dictionary<string, Expression<Func<OrdemServico, object?>>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoPaginado);

            var result = await _queryHandler.Handle(query, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.NotNull(result.Valor);
            Assert.Single(result.Valor!.Itens);
            Assert.Equal(ordem.Id, result.Valor.Itens.First().Id);
        }
    }
}
