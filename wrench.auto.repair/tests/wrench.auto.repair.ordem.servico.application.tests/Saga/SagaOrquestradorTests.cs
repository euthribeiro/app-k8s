using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Wrench.Contracts;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.ordem.servico.application.Saga;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Enums;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.application.tests.Saga
{
    public class SagaOrquestradorTests
    {
        private static readonly DateTime Agora = new(2026, 11, 14, 10, 0, 0, DateTimeKind.Utc);
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly Mock<ISagaOrdemServicoRepository> _sagaRepository = new();
        private readonly Mock<IOrdemServicoRepository> _ordemServicoRepository = new();
        private readonly Mock<IOutboxSaga> _outbox = new();
        private readonly Mock<IInboxSaga> _inbox = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly SagaOptions _opcoes = new();

        private readonly List<(string Tipo, ISagaMensagem Conteudo)> _publicadas = [];

        private readonly SagaOrquestrador _orquestrador;

        public SagaOrquestradorTests()
        {
            _unitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(true);
            _sagaRepository.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);

            _inbox.Setup(i => i.JaProcessadaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _outbox.Setup(o => o.Enfileirar(It.IsAny<string>(), It.IsAny<GerarOrcamento>()))
                .Callback<string, GerarOrcamento>((tipo, conteudo) => _publicadas.Add((tipo, conteudo)));
            _outbox.Setup(o => o.Enfileirar(It.IsAny<string>(), It.IsAny<CancelarOrcamento>()))
                .Callback<string, CancelarOrcamento>((tipo, conteudo) => _publicadas.Add((tipo, conteudo)));
            _outbox.Setup(o => o.Enfileirar(It.IsAny<string>(), It.IsAny<EstornarPagamento>()))
                .Callback<string, EstornarPagamento>((tipo, conteudo) => _publicadas.Add((tipo, conteudo)));
            _outbox.Setup(o => o.Enfileirar(It.IsAny<string>(), It.IsAny<IniciarExecucao>()))
                .Callback<string, IniciarExecucao>((tipo, conteudo) => _publicadas.Add((tipo, conteudo)));
            _outbox.Setup(o => o.Enfileirar(It.IsAny<string>(), It.IsAny<CancelarExecucao>()))
                .Callback<string, CancelarExecucao>((tipo, conteudo) => _publicadas.Add((tipo, conteudo)));

            _orquestrador = new SagaOrquestrador(
                _sagaRepository.Object,
                _ordemServicoRepository.Object,
                _outbox.Object,
                _inbox.Object,
                Options.Create(_opcoes),
                new RelogioFixo(Agora),
                NullLogger<SagaOrquestrador>.Instance);
        }

        [Fact(DisplayName = "Iniciar grava a Saga e enfileira GerarOrcamento")]
        [Trait("Saga", "Application")]
        public async Task Iniciar_DeveEnfileirarGerarOrcamento()
        {
            var ordemServico = CriarOrdemServicoEmDiagnostico();
            ordemServico.AdicionarDiagnostico("Troca de embreagem", 800m);

            await _orquestrador.IniciarAsync(ordemServico, "corr-1", CancellationToken.None);

            _sagaRepository.Verify(r => r.Adicionar(It.IsAny<SagaOrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

            var (tipo, conteudo) = Assert.Single(_publicadas);
            Assert.Equal(MensagemTipos.GerarOrcamento, tipo);

            var comando = Assert.IsType<GerarOrcamento>(conteudo);
            Assert.Equal(ordemServico.Id, comando.OrdemServicoId);
            Assert.Equal("corr-1", comando.CorrelationId);
            Assert.Equal(800m, comando.ValorServico);
        }

        [Fact(DisplayName = "Mensagem repetida é ignorada pelo inbox")]
        [Trait("Saga", "Application")]
        public async Task Processar_MensagemRepetida_DeveIgnorar()
        {
            _inbox.Setup(i => i.JaProcessadaAsync("msg-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.OrcamentoCriado, new OrcamentoCriado(Guid.NewGuid(), "corr-1", Guid.NewGuid(), 10m, Agora)),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Ignorada, resultado);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Mensagem de tipo desconhecido não é processada")]
        [Trait("Saga", "Application")]
        public async Task Processar_TipoDesconhecido_DeveRecusar()
        {
            var resultado = await _orquestrador.ProcessarAsync(
                new MensagemRecebida("msg-1", "NaoExiste", "{}", "corr-1", null, 1),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.TipoDesconhecido, resultado);
        }

        [Fact(DisplayName = "Evento sem Saga correspondente devolve SagaNaoEncontrada")]
        [Trait("Saga", "Application")]
        public async Task Processar_SemSaga_DeveDevolverNaoEncontrada()
        {
            var ordemServicoId = Guid.NewGuid();

            _sagaRepository.Setup(r => r.ObterPorOrdemServicoAsync(ordemServicoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SagaOrdemServico?)null);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.OrcamentoCriado, new OrcamentoCriado(ordemServicoId, "corr-1", Guid.NewGuid(), 10m, Agora)),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.SagaNaoEncontrada, resultado);
        }

        [Fact(DisplayName = "PagamentoAprovado aprova a OS e enfileira IniciarExecucao")]
        [Trait("Saga", "Application")]
        public async Task Processar_PagamentoAprovado_DeveIniciarExecucao()
        {
            var ordemServico = CriarOrdemServicoAguardandoAprovacao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoPagamento);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.PagamentoAprovado,
                    new PagamentoAprovado(ordemServico.Id, saga.CorrelationId, Guid.NewGuid(), 800m, Agora)),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(SagaEstado.AguardandoExecucao, saga.Estado);
            Assert.Equal(OrdemServicoStatus.EmExecucao, ordemServico.Status);

            var (tipo, conteudo) = Assert.Single(_publicadas);
            Assert.Equal(MensagemTipos.IniciarExecucao, tipo);
            Assert.Equal(800m, Assert.IsType<IniciarExecucao>(conteudo).ValorAprovado);

            _inbox.Verify(i => i.Registrar("msg-1", MensagemTipos.PagamentoAprovado, Agora), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "ExecucaoConcluida finaliza a ordem de serviço")]
        [Trait("Saga", "Application")]
        public async Task Processar_ExecucaoConcluida_DeveFinalizar()
        {
            var ordemServico = CriarOrdemServicoEmExecucao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoExecucao);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.ExecucaoConcluida,
                    new ExecucaoConcluida(ordemServico.Id, saga.CorrelationId, Agora)),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(SagaEstado.Concluida, saga.Estado);
            Assert.Equal(OrdemServicoStatus.Finalizada, ordemServico.Status);
            Assert.Empty(_publicadas);
        }

        [Fact(DisplayName = "OrcamentoFalhou devolve a OS para o diagnóstico sem cancelar")]
        [Trait("Saga", "Application")]
        public async Task Processar_OrcamentoFalhou_DeveVoltarParaDiagnostico()
        {
            var ordemServico = CriarOrdemServicoAguardandoAprovacao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoOrcamento);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.OrcamentoFalhou,
                    new OrcamentoFalhou(ordemServico.Id, saga.CorrelationId, "Billing indisponivel")),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(SagaEstado.Compensada, saga.Estado);
            Assert.Equal(OrdemServicoStatus.EmDiagnostico, ordemServico.Status);
            Assert.Empty(_publicadas);
        }

        [Fact(DisplayName = "OrcamentoRecusado cancela o orçamento e a ordem de serviço")]
        [Trait("Saga", "Application")]
        public async Task Processar_OrcamentoRecusado_DeveCancelar()
        {
            var ordemServico = CriarOrdemServicoAguardandoAprovacao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoAprovacao);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.OrcamentoRecusado,
                    new OrcamentoRecusado(ordemServico.Id, saga.CorrelationId, "Cliente recusou", Agora)),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(SagaEstado.Compensada, saga.Estado);
            Assert.Equal(OrdemServicoStatus.Cancelada, ordemServico.Status);

            var (tipo, _) = Assert.Single(_publicadas);
            Assert.Equal(MensagemTipos.CancelarOrcamento, tipo);
        }

        [Fact(DisplayName = "PagamentoFalhou cancela o orçamento e a ordem de serviço")]
        [Trait("Saga", "Application")]
        public async Task Processar_PagamentoFalhou_DeveCancelar()
        {
            var ordemServico = CriarOrdemServicoAguardandoAprovacao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoPagamento);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.PagamentoFalhou,
                    new PagamentoFalhou(ordemServico.Id, saga.CorrelationId, "Pix expirou")),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(OrdemServicoStatus.Cancelada, ordemServico.Status);
            Assert.Equal(MensagemTipos.CancelarOrcamento, Assert.Single(_publicadas).Tipo);
        }

        [Fact(DisplayName = "ExecucaoFalhou estorna o pagamento, tira da fila e cancela a OS")]
        [Trait("Saga", "Application")]
        public async Task Processar_ExecucaoFalhou_DeveEstornar()
        {
            var ordemServico = CriarOrdemServicoEmExecucao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoExecucao, comPagamento: true);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-1", MensagemTipos.ExecucaoFalhou,
                    new ExecucaoFalhou(ordemServico.Id, saga.CorrelationId, "Sem mecanico disponivel")),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(SagaEstado.Compensando, saga.Estado);
            Assert.Equal(OrdemServicoStatus.Cancelada, ordemServico.Status);

            var tipos = _publicadas.Select(mensagem => mensagem.Tipo).ToList();
            Assert.Contains(MensagemTipos.CancelarExecucao, tipos);
            Assert.Contains(MensagemTipos.EstornarPagamento, tipos);
        }

        [Fact(DisplayName = "PagamentoEstornado fecha a compensação")]
        [Trait("Saga", "Application")]
        public async Task Processar_PagamentoEstornado_DeveFecharCompensacao()
        {
            var ordemServico = CriarOrdemServicoEmExecucao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoExecucao, comPagamento: true);
            saga.Compensar("ExecucaoFalhou", "Sem mecanico", Agora, _opcoes.PrazoEstorno);
            _publicadas.Clear();

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-2", MensagemTipos.PagamentoEstornado,
                    new PagamentoEstornado(ordemServico.Id, saga.CorrelationId, Guid.NewGuid(), 800m, Agora)),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(SagaEstado.Compensada, saga.Estado);
        }

        [Fact(DisplayName = "EstornoFalhou deixa a Saga em Falha")]
        [Trait("Saga", "Application")]
        public async Task Processar_EstornoFalhou_DeveMarcarFalha()
        {
            var ordemServico = CriarOrdemServicoEmExecucao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoExecucao, comPagamento: true);
            saga.Compensar("ExecucaoFalhou", "Sem mecanico", Agora, _opcoes.PrazoEstorno);

            var resultado = await _orquestrador.ProcessarAsync(
                Mensagem("msg-2", MensagemTipos.EstornoFalhou,
                    new EstornoFalhou(ordemServico.Id, saga.CorrelationId, "Mercado Pago recusou")),
                CancellationToken.None);

            Assert.Equal(ResultadoSaga.Processada, resultado);
            Assert.Equal(SagaEstado.Falha, saga.Estado);
        }

        [Fact(DisplayName = "Timeout aplica a mesma compensação da falha da etapa")]
        [Trait("Saga", "Application")]
        public async Task AplicarTimeouts_DeveCompensar()
        {
            var ordemServico = CriarOrdemServicoAguardandoAprovacao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoPagamento);

            _sagaRepository.Setup(r => r.ObterExpiradasAsync(Agora, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([saga]);

            var compensadas = await _orquestrador.AplicarTimeoutsAsync(CancellationToken.None);

            Assert.Equal(1, compensadas);
            Assert.Equal(SagaEstado.Compensada, saga.Estado);
            Assert.Equal(OrdemServicoStatus.Cancelada, ordemServico.Status);
            Assert.Equal(MensagemTipos.CancelarOrcamento, Assert.Single(_publicadas).Tipo);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Sem Saga expirada o worker não confirma transação")]
        [Trait("Saga", "Application")]
        public async Task AplicarTimeouts_SemExpiradas_NaoDeveComitar()
        {
            _sagaRepository.Setup(r => r.ObterExpiradasAsync(Agora, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var compensadas = await _orquestrador.AplicarTimeoutsAsync(CancellationToken.None);

            Assert.Equal(0, compensadas);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact(DisplayName = "Timeout do estorno deixa a Saga em Falha")]
        [Trait("Saga", "Application")]
        public async Task AplicarTimeouts_NoEstorno_DeveMarcarFalha()
        {
            var ordemServico = CriarOrdemServicoEmExecucao();
            var saga = PrepararSaga(ordemServico.Id, SagaEstado.AguardandoExecucao, comPagamento: true);
            saga.Compensar("ExecucaoFalhou", "Sem mecanico", Agora, _opcoes.PrazoEstorno);
            _publicadas.Clear();

            _sagaRepository.Setup(r => r.ObterExpiradasAsync(Agora, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([saga]);

            var compensadas = await _orquestrador.AplicarTimeoutsAsync(CancellationToken.None);

            Assert.Equal(1, compensadas);
            Assert.Equal(SagaEstado.Falha, saga.Estado);
        }

        private SagaOrdemServico PrepararSaga(Guid ordemServicoId, SagaEstado estado, bool comPagamento = false)
        {
            var saga = new SagaOrdemServico(ordemServicoId, "corr-1", Agora, _opcoes.PrazoOrcamento);

            if (estado != SagaEstado.AguardandoOrcamento)
                saga.AoCriarOrcamento(Guid.NewGuid(), Agora, _opcoes.PrazoAprovacao);

            if (estado is SagaEstado.AguardandoPagamento or SagaEstado.AguardandoExecucao)
                saga.AoAprovarOrcamento(Guid.NewGuid(), 800m, Agora, _opcoes.PrazoPagamento);

            if (estado == SagaEstado.AguardandoExecucao || comPagamento)
                saga.AoAprovarPagamento(Guid.NewGuid(), 800m, Agora, _opcoes.PrazoExecucao);

            _sagaRepository.Setup(r => r.ObterPorOrdemServicoAsync(ordemServicoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saga);

            return saga;
        }

        private OrdemServico CriarOrdemServicoEmDiagnostico()
        {
            var ordemServico = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "Barulho no motor", OrdemServicoStatus.Recebida, Agora);
            ordemServico.SolicitaDiagnostico();

            _ordemServicoRepository.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordemServico);

            return ordemServico;
        }

        private OrdemServico CriarOrdemServicoAguardandoAprovacao()
        {
            var ordemServico = CriarOrdemServicoEmDiagnostico();
            ordemServico.AdicionarDiagnostico("Troca de embreagem", 800m);

            return ordemServico;
        }

        private OrdemServico CriarOrdemServicoEmExecucao()
        {
            var ordemServico = CriarOrdemServicoAguardandoAprovacao();
            ordemServico.AprovarOrcamento();

            return ordemServico;
        }

        private static MensagemRecebida Mensagem<TContrato>(string messageId, string tipo, TContrato conteudo)
            where TContrato : ISagaMensagem =>
            new(messageId, tipo, JsonSerializer.Serialize(conteudo, Json), conteudo.CorrelationId, null, 1);

        private sealed class RelogioFixo(DateTime agora) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => new(agora, TimeSpan.Zero);
        }
    }
}
