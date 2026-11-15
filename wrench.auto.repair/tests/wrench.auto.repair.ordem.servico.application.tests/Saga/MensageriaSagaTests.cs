using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS.Model;
using AtributoSqs = Amazon.SQS.Model.MessageAttributeValue;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Wrench.Contracts;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;
using wrench.auto.repair.ordem.servico.infra.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.application.tests.Saga
{
    public class MensageriaSagaTests
    {
        private static readonly DateTime Agora = new(2026, 11, 14, 10, 0, 0, DateTimeKind.Utc);

        private static MensagemSaga Mensagem(string tipo = MensagemTipos.CancelarOrcamento) =>
            MensagemSaga.Criar(tipo, new CancelarOrcamento(Guid.NewGuid(), "corr-1", "Cliente recusou"), Agora);

        [Fact(DisplayName = "MensagemSaga nasce do contrato com payload em camelCase")]
        [Trait("Saga", "Mensageria")]
        public void MensagemSaga_DeveSerializarOContrato()
        {
            var contrato = new CancelarOrcamento(Guid.NewGuid(), "corr-1", "Cliente recusou");

            var mensagem = MensagemSaga.Criar(MensagemTipos.CancelarOrcamento, contrato, Agora);

            Assert.NotEqual(Guid.Empty, mensagem.Id);
            Assert.Equal(contrato.OrdemServicoId, mensagem.OrdemServicoId);
            Assert.Equal("corr-1", mensagem.CorrelationId);
            Assert.Equal(Agora, mensagem.CriadaEm);
            Assert.Contains("ordemServicoId", mensagem.Payload);
            Assert.Contains("Cliente recusou", mensagem.Payload);
        }

        [Fact(DisplayName = "Publicação no SNS leva corpo e atributos da mensagem")]
        [Trait("Saga", "Mensageria")]
        public async Task Publicar_DeveEnviarCorpoEAtributos()
        {
            var sns = new Mock<IAmazonSimpleNotificationService>();
            PublishRequest? enviado = null;

            sns.Setup(c => c.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
                .Callback<PublishRequest, CancellationToken>((requisicao, _) => enviado = requisicao)
                .ReturnsAsync(new PublishResponse());

            var publicador = new SnsPublicadorSaga(sns.Object, Options.Create(new SnsOptions { TopicArn = "arn:topico" }));

            var mensagem = Mensagem() with { Traceparent = "00-trace-span-01" };

            await publicador.PublicarAsync(mensagem, CancellationToken.None);

            Assert.NotNull(enviado);
            Assert.Equal("arn:topico", enviado!.TopicArn);
            Assert.Equal(mensagem.Payload, enviado.Message);
            Assert.Equal(MensagemTipos.CancelarOrcamento, enviado.MessageAttributes[AtributosMensagem.Tipo].StringValue);
            Assert.Equal(mensagem.Id.ToString(), enviado.MessageAttributes[AtributosMensagem.MessageId].StringValue);
            Assert.Equal("corr-1", enviado.MessageAttributes[AtributosMensagem.CorrelationId].StringValue);
            Assert.Equal("00-trace-span-01", enviado.MessageAttributes[AtributosMensagem.Traceparent].StringValue);
        }

        [Fact(DisplayName = "Publicação sem tópico configurado falha antes de chamar a AWS")]
        [Trait("Saga", "Mensageria")]
        public async Task Publicar_SemTopico_DeveFalhar()
        {
            var sns = new Mock<IAmazonSimpleNotificationService>();
            var publicador = new SnsPublicadorSaga(sns.Object, Options.Create(new SnsOptions()));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => publicador.PublicarAsync(Mensagem(), CancellationToken.None));

            sns.Verify(c => c.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Mensagem sem traceparent não envia o atributo")]
        [Trait("Saga", "Mensageria")]
        public async Task Publicar_SemTraceparent_NaoDeveEnviarAtributo()
        {
            var sns = new Mock<IAmazonSimpleNotificationService>();
            PublishRequest? enviado = null;

            sns.Setup(c => c.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
                .Callback<PublishRequest, CancellationToken>((requisicao, _) => enviado = requisicao)
                .ReturnsAsync(new PublishResponse());

            var publicador = new SnsPublicadorSaga(sns.Object, Options.Create(new SnsOptions { TopicArn = "arn:topico" }));

            await publicador.PublicarAsync(Mensagem() with { Traceparent = null }, CancellationToken.None);

            Assert.False(enviado!.MessageAttributes.ContainsKey(AtributosMensagem.Traceparent));
        }

        [Fact(DisplayName = "Conversão lê tipo, correlação, traceparent e tentativas")]
        [Trait("Saga", "Mensageria")]
        public void Converter_DeveLerOsAtributos()
        {
            var mensagem = new Message
            {
                MessageId = "sqs-1",
                Body = "{}",
                MessageAttributes = new Dictionary<string, AtributoSqs>
                {
                    [AtributosMensagem.Tipo] = new() { StringValue = MensagemTipos.ExecucaoConcluida },
                    [AtributosMensagem.MessageId] = new() { StringValue = "outbox-1" },
                    [AtributosMensagem.CorrelationId] = new() { StringValue = "corr-9" },
                    [AtributosMensagem.Traceparent] = new() { StringValue = "00-trace-span-01" }
                },
                Attributes = new Dictionary<string, string>
                {
                    [ConversorMensagemSqs.ContagemDeEntregas] = "3"
                }
            };

            var recebida = ConversorMensagemSqs.Converter(mensagem);

            Assert.NotNull(recebida);
            Assert.Equal("outbox-1", recebida!.MessageId);
            Assert.Equal(MensagemTipos.ExecucaoConcluida, recebida.Tipo);
            Assert.Equal("corr-9", recebida.CorrelationId);
            Assert.Equal("00-trace-span-01", recebida.Traceparent);
            Assert.Equal(3, recebida.TentativasDeEntrega);
        }

        [Fact(DisplayName = "Sem messageId do outbox a conversão usa o id do SQS e uma tentativa")]
        [Trait("Saga", "Mensageria")]
        public void Converter_SemMetadadosOpcionais_DeveUsarPadroes()
        {
            var mensagem = new Message
            {
                MessageId = "sqs-1",
                Body = "{}",
                MessageAttributes = new Dictionary<string, AtributoSqs>
                {
                    [AtributosMensagem.Tipo] = new() { StringValue = MensagemTipos.ExecucaoFalhou }
                }
            };

            var recebida = ConversorMensagemSqs.Converter(mensagem);

            Assert.Equal("sqs-1", recebida!.MessageId);
            Assert.Null(recebida.CorrelationId);
            Assert.Null(recebida.Traceparent);
            Assert.Equal(1, recebida.TentativasDeEntrega);
        }

        [Theory(DisplayName = "Mensagem sem o atributo tipo não é convertida")]
        [Trait("Saga", "Mensageria")]
        [InlineData(null)]
        [InlineData("")]
        public void Converter_SemTipo_DeveDevolverNulo(string? tipo)
        {
            var atributos = new Dictionary<string, AtributoSqs>();

            if (tipo is not null)
                atributos[AtributosMensagem.Tipo] = new AtributoSqs { StringValue = tipo };

            var recebida = ConversorMensagemSqs.Converter(new Message
            {
                MessageId = "sqs-1",
                Body = "{}",
                MessageAttributes = atributos
            });

            Assert.Null(recebida);
        }

        [Fact(DisplayName = "Publicação do outbox marca cada mensagem entregue")]
        [Trait("Saga", "Mensageria")]
        public async Task PublicarPendentes_DeveMarcarPublicadas()
        {
            var primeira = Mensagem();
            var segunda = Mensagem(MensagemTipos.EstornarPagamento);

            var outbox = new Mock<IOutboxSaga>();
            outbox.Setup(o => o.ObterPendentesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([primeira, segunda]);
            outbox.Setup(o => o.ContarPendentesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

            var publicador = new Mock<IPublicadorSaga>();

            var publicacao = new PublicacaoOutbox(outbox.Object, publicador.Object, new RelogioFixo(Agora),
                NullLogger<PublicacaoOutbox>.Instance);

            var publicadas = await publicacao.PublicarPendentesAsync(25, CancellationToken.None);

            Assert.Equal(2, publicadas);
            Assert.Equal(2, publicacao.Pendentes);
            outbox.Verify(o => o.MarcarPublicadaAsync(primeira.Id, Agora, It.IsAny<CancellationToken>()), Times.Once);
            outbox.Verify(o => o.MarcarPublicadaAsync(segunda.Id, Agora, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Mensagem que falha ao publicar continua pendente")]
        [Trait("Saga", "Mensageria")]
        public async Task PublicarPendentes_ComFalha_DeveManterPendente()
        {
            var comFalha = Mensagem();
            var comSucesso = Mensagem(MensagemTipos.EstornarPagamento);

            var outbox = new Mock<IOutboxSaga>();
            outbox.Setup(o => o.ObterPendentesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([comFalha, comSucesso]);
            outbox.Setup(o => o.ContarPendentesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

            var publicador = new Mock<IPublicadorSaga>();
            publicador.Setup(p => p.PublicarAsync(comFalha, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("SNS fora do ar"));

            var publicacao = new PublicacaoOutbox(outbox.Object, publicador.Object, new RelogioFixo(Agora),
                NullLogger<PublicacaoOutbox>.Instance);

            var publicadas = await publicacao.PublicarPendentesAsync(25, CancellationToken.None);

            Assert.Equal(1, publicadas);
            outbox.Verify(o => o.MarcarPublicadaAsync(comFalha.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            outbox.Verify(o => o.MarcarPublicadaAsync(comSucesso.Id, Agora, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Outbox vazio não chama o publicador")]
        [Trait("Saga", "Mensageria")]
        public async Task PublicarPendentes_SemPendentes_NaoDevePublicar()
        {
            var outbox = new Mock<IOutboxSaga>();
            outbox.Setup(o => o.ObterPendentesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
            outbox.Setup(o => o.ContarPendentesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

            var publicador = new Mock<IPublicadorSaga>();

            var publicacao = new PublicacaoOutbox(outbox.Object, publicador.Object, new RelogioFixo(Agora),
                NullLogger<PublicacaoOutbox>.Instance);

            Assert.Equal(0, await publicacao.PublicarPendentesAsync(25, CancellationToken.None));
            publicador.Verify(p => p.PublicarAsync(It.IsAny<MensagemSaga>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Prazos da Saga viram TimeSpan a partir dos minutos configurados")]
        [Trait("Saga", "Mensageria")]
        public void SagaOptions_DeveConverterOsPrazos()
        {
            var opcoes = new SagaOptions
            {
                PrazoOrcamentoMinutos = 10,
                PrazoAprovacaoMinutos = 20,
                PrazoPagamentoMinutos = 30,
                PrazoExecucaoMinutos = 40,
                PrazoEstornoMinutos = 50
            };

            Assert.Equal(TimeSpan.FromMinutes(10), opcoes.PrazoOrcamento);
            Assert.Equal(TimeSpan.FromMinutes(20), opcoes.PrazoAprovacao);
            Assert.Equal(TimeSpan.FromMinutes(30), opcoes.PrazoPagamento);
            Assert.Equal(TimeSpan.FromMinutes(40), opcoes.PrazoExecucao);
            Assert.Equal(TimeSpan.FromMinutes(50), opcoes.PrazoEstorno);
        }

        [Fact(DisplayName = "Contadores e spans da Saga ficam no mesmo meter consultado pelo New Relic")]
        [Trait("Saga", "Mensageria")]
        public void SagaMetrics_DevePublicarNoMeterEsperado()
        {
            Assert.Equal("Wrench.Saga", SagaMetrics.MeterName);
            Assert.Equal("Wrench.Saga", SagaMetrics.ActivitySourceName);

            SagaMetrics.SagaIniciada();
            SagaMetrics.SagaConcluida(1200);
            SagaMetrics.SagaCompensada(nameof(SagaEtapa.Execucao), 1500);
            SagaMetrics.TimeoutDaEtapa(nameof(SagaEstado.AguardandoPagamento));
            SagaMetrics.MensagemRecebida(MensagemTipos.PagamentoAprovado);
            SagaMetrics.MensagemFalhou(MensagemTipos.PagamentoAprovado);
            SagaMetrics.MensagemParaDlq(MensagemTipos.PagamentoAprovado);
            SagaMetrics.MensagemPublicada(MensagemTipos.IniciarExecucao);
            SagaMetrics.RegistrarOutboxPendentes(() => 7);
        }

        private sealed class RelogioFixo(DateTime agora) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => new(agora, TimeSpan.Zero);
        }
    }
}
