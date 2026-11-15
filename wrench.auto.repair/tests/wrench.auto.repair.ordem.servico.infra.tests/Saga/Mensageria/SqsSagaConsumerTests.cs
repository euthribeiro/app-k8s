using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Wrench.Contracts;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;
using wrench.auto.repair.ordem.servico.infra.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.infra.tests.Saga.Mensageria
{
    public class SqsSagaConsumerTests
    {
        private const string FilaDaSaga = "https://sqs.us-east-1.amazonaws.com/000000000000/wrench-os-saga";
        private const string ReceiptHandle = "receipt-handle-1";

        [Fact(DisplayName = "Sem Sqs:QueueUrl o consumidor nao le a fila")]
        [Trait("Saga", "Consumidor")]
        public async Task ExecuteAsync_DeveEncerrarSemLerAFila_QuandoQueueUrlNaoEstiverConfigurada()
        {
            var sqs = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var orquestrador = new Mock<ISagaOrquestrador>(MockBehavior.Strict);

            var consumidor = CriarConsumidor(sqs, orquestrador, new SqsOptions { QueueUrl = string.Empty });

            await consumidor.StartAsync(CancellationToken.None);
            await consumidor.StopAsync(CancellationToken.None);

            sqs.Verify(c => c.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Mensagem processada e apagada da fila")]
        [Trait("Saga", "Consumidor")]
        public async Task ProcessarAsync_DeveApagarDaFila_QuandoOrquestradorProcessaAMensagem()
        {
            var apagada = new TaskCompletionSource();

            var sqs = CriarSqs(apagada, MensagemComTipo(SagaMensagemTipo(), tentativas: 1));
            var orquestrador = new Mock<ISagaOrquestrador>();

            orquestrador
                .Setup(o => o.ProcessarAsync(It.IsAny<MensagemRecebida>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ResultadoSaga.Processada);

            await ExecutarAteAsync(sqs, orquestrador, apagada);

            sqs.Verify(c => c.DeleteMessageAsync(FilaDaSaga, ReceiptHandle, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact(DisplayName = "Mensagem sem o atributo tipo e descartada sem chamar o orquestrador")]
        [Trait("Saga", "Consumidor")]
        public async Task ProcessarAsync_DeveApagarSemOrquestrar_QuandoMensagemNaoTiverTipo()
        {
            var apagada = new TaskCompletionSource();

            var mensagem = new Message
            {
                MessageId = "sem-tipo",
                Body = "{}",
                ReceiptHandle = ReceiptHandle,
                MessageAttributes = []
            };

            var sqs = CriarSqs(apagada, mensagem);
            var orquestrador = new Mock<ISagaOrquestrador>(MockBehavior.Strict);

            await ExecutarAteAsync(sqs, orquestrador, apagada);

            orquestrador.Verify(o => o.ProcessarAsync(It.IsAny<MensagemRecebida>(), It.IsAny<CancellationToken>()), Times.Never);
            sqs.Verify(c => c.DeleteMessageAsync(FilaDaSaga, ReceiptHandle, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact(DisplayName = "Saga ainda inexistente mantem a mensagem na fila para reentrega")]
        [Trait("Saga", "Consumidor")]
        public async Task ProcessarAsync_NaoDeveApagar_QuandoSagaNaoExisteEAindaHaTentativas()
        {
            var orquestrado = new TaskCompletionSource();

            var sqs = CriarSqs(null, MensagemComTipo(SagaMensagemTipo(), tentativas: 1));
            var orquestrador = new Mock<ISagaOrquestrador>();

            orquestrador
                .Setup(o => o.ProcessarAsync(It.IsAny<MensagemRecebida>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ResultadoSaga.SagaNaoEncontrada)
                .Callback(() => orquestrado.TrySetResult());

            await ExecutarAteAsync(sqs, orquestrador, orquestrado);

            sqs.Verify(c => c.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Saga inexistente apos esgotar as tentativas sai da fila")]
        [Trait("Saga", "Consumidor")]
        public async Task ProcessarAsync_DeveApagar_QuandoSagaNaoExisteEAsTentativasAcabaram()
        {
            var apagada = new TaskCompletionSource();

            var sqs = CriarSqs(apagada, MensagemComTipo(SagaMensagemTipo(), tentativas: 5));
            var orquestrador = new Mock<ISagaOrquestrador>();

            orquestrador
                .Setup(o => o.ProcessarAsync(It.IsAny<MensagemRecebida>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ResultadoSaga.SagaNaoEncontrada);

            await ExecutarAteAsync(sqs, orquestrador, apagada);

            sqs.Verify(c => c.DeleteMessageAsync(FilaDaSaga, ReceiptHandle, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact(DisplayName = "Falha no processamento devolve a mensagem para a fila")]
        [Trait("Saga", "Consumidor")]
        public async Task ProcessarAsync_NaoDeveApagar_QuandoOrquestradorLancaExcecao()
        {
            var orquestrado = new TaskCompletionSource();

            var sqs = CriarSqs(null, MensagemComTipo(SagaMensagemTipo(), tentativas: 2));
            var orquestrador = new Mock<ISagaOrquestrador>();

            orquestrador
                .Setup(o => o.ProcessarAsync(It.IsAny<MensagemRecebida>(), It.IsAny<CancellationToken>()))
                .Callback(() => orquestrado.TrySetResult())
                .ThrowsAsync(new InvalidOperationException("falha simulada"));

            await ExecutarAteAsync(sqs, orquestrador, orquestrado);

            sqs.Verify(c => c.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "O corpo e os metadados da mensagem chegam ao orquestrador")]
        [Trait("Saga", "Consumidor")]
        public async Task ProcessarAsync_DeveRepassarCorpoEMetadados_AoOrquestrador()
        {
            var orquestrado = new TaskCompletionSource();
            MensagemRecebida? recebida = null;

            var sqs = CriarSqs(null, MensagemComTipo(SagaMensagemTipo(), tentativas: 3));
            var orquestrador = new Mock<ISagaOrquestrador>();

            orquestrador
                .Setup(o => o.ProcessarAsync(It.IsAny<MensagemRecebida>(), It.IsAny<CancellationToken>()))
                .Callback<MensagemRecebida, CancellationToken>((mensagem, _) =>
                {
                    recebida = mensagem;
                    orquestrado.TrySetResult();
                })
                .ReturnsAsync(ResultadoSaga.Processada);

            await ExecutarAteAsync(sqs, orquestrador, orquestrado);

            Assert.NotNull(recebida);
            Assert.Equal(SagaMensagemTipo(), recebida!.Tipo);
            Assert.Equal("{\"ordemServicoId\":1}", recebida.Corpo);
            Assert.Equal("correlacao-1", recebida.CorrelationId);
            Assert.Equal(3, recebida.TentativasDeEntrega);
        }

        private static string SagaMensagemTipo() => "OrcamentoCriado";

        private static Message MensagemComTipo(string tipo, int tentativas) =>
            new()
            {
                MessageId = "mensagem-1",
                Body = "{\"ordemServicoId\":1}",
                ReceiptHandle = ReceiptHandle,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    [AtributosMensagem.Tipo] = new() { DataType = "String", StringValue = tipo },
                    [AtributosMensagem.MessageId] = new() { DataType = "String", StringValue = "mensagem-1" },
                    [AtributosMensagem.CorrelationId] = new() { DataType = "String", StringValue = "correlacao-1" }
                },
                Attributes = new Dictionary<string, string>
                {
                    [ConversorMensagemSqs.ContagemDeEntregas] = tentativas.ToString()
                }
            };

        /// <summary>
        /// Entrega o lote na primeira leitura e vazio nas seguintes, para que o laço do
        /// <see cref="SqsSagaConsumer"/> não fique girando enquanto o teste espera o sinal.
        /// </summary>
        private static Mock<IAmazonSQS> CriarSqs(TaskCompletionSource? apagada, params Message[] mensagens)
        {
            var sqs = new Mock<IAmazonSQS>();
            var primeiraLeitura = true;

            sqs
                .Setup(c => c.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    if (!primeiraLeitura) return new ReceiveMessageResponse { Messages = [] };

                    primeiraLeitura = false;

                    return new ReceiveMessageResponse { Messages = [.. mensagens] };
                });

            sqs
                .Setup(c => c.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteMessageResponse())
                .Callback(() => apagada?.TrySetResult());

            return sqs;
        }

        private static SqsSagaConsumer CriarConsumidor(
            Mock<IAmazonSQS> sqs,
            Mock<ISagaOrquestrador> orquestrador,
            SqsOptions opcoes)
        {
            var servicos = new ServiceCollection();
            servicos.AddScoped(_ => orquestrador.Object);

            var escopos = servicos.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

            return new SqsSagaConsumer(
                sqs.Object,
                escopos,
                Options.Create(opcoes),
                NullLogger<SqsSagaConsumer>.Instance);
        }

        private static async Task ExecutarAteAsync(
            Mock<IAmazonSQS> sqs,
            Mock<ISagaOrquestrador> orquestrador,
            TaskCompletionSource sinal)
        {
            var opcoes = new SqsOptions
            {
                QueueUrl = FilaDaSaga,
                EsperaLongaSegundos = 0,
                TentativasAteDlq = 5
            };

            var consumidor = CriarConsumidor(sqs, orquestrador, opcoes);

            await consumidor.StartAsync(CancellationToken.None);

            await sinal.Task.WaitAsync(TimeSpan.FromSeconds(10));

            await consumidor.StopAsync(CancellationToken.None);
        }
    }
}
