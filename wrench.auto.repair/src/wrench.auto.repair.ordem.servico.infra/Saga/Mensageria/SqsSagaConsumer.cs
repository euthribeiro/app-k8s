using System.Diagnostics;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.infra.Saga.Mensageria
{
    /// <summary>
    /// Lê a fila da Saga por long polling e entrega cada mensagem ao orquestrador. A mensagem só
    /// é apagada da fila quando o processamento confirma a transação; qualquer exceção a devolve
    /// para a fila e, esgotadas as tentativas, o SQS a move para a DLQ.
    /// </summary>
    public class SqsSagaConsumer(
        IAmazonSQS sqs,
        IServiceScopeFactory escopos,
        IOptions<SqsOptions> opcoes,
        ILogger<SqsSagaConsumer> logger) : BackgroundService
    {
        private readonly SqsOptions _opcoes = opcoes.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_opcoes.QueueUrl))
            {
                logger.LogWarning("A fila da Saga (Sqs:QueueUrl) nao foi configurada; o consumidor nao sera iniciado.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumirLoteAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception excecao)
                {
                    logger.LogError(excecao, "Falha ao ler a fila da Saga; nova tentativa em instantes.");

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ConsumirLoteAsync(CancellationToken cancellationToken)
        {
            var resposta = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _opcoes.QueueUrl,
                MaxNumberOfMessages = _opcoes.MensagensPorLeitura,
                WaitTimeSeconds = _opcoes.EsperaLongaSegundos,
                MessageAttributeNames = ["All"],
                MessageSystemAttributeNames = [ConversorMensagemSqs.ContagemDeEntregas]
            }, cancellationToken);

            foreach (var mensagem in resposta.Messages ?? [])
            {
                await ProcessarAsync(mensagem, cancellationToken);
            }
        }

        private async Task ProcessarAsync(Message mensagem, CancellationToken cancellationToken)
        {
            var recebida = ConversorMensagemSqs.Converter(mensagem);

            if (recebida is null)
            {
                logger.LogWarning("Mensagem {MessageId} sem o atributo tipo; removida da fila.", mensagem.MessageId);
                await ApagarAsync(mensagem, cancellationToken);
                return;
            }

            using var atividade = IniciarAtividade(recebida);

            try
            {
                using var escopo = escopos.CreateScope();

                var orquestrador = escopo.ServiceProvider.GetRequiredService<ISagaOrquestrador>();

                var resultado = await orquestrador.ProcessarAsync(recebida, cancellationToken);

                if (resultado == ResultadoSaga.SagaNaoEncontrada && recebida.TentativasDeEntrega < _opcoes.TentativasAteDlq)
                {
                    logger.LogWarning(
                        "Mensagem {Tipo} chegou antes da Saga existir; sera reentregue (tentativa {Tentativa}).",
                        recebida.Tipo, recebida.TentativasDeEntrega);

                    return;
                }

                await ApagarAsync(mensagem, cancellationToken);
            }
            catch (Exception excecao)
            {
                SagaMetrics.MensagemFalhou(recebida.Tipo);

                if (recebida.TentativasDeEntrega >= _opcoes.TentativasAteDlq)
                    SagaMetrics.MensagemParaDlq(recebida.Tipo);

                logger.LogError(excecao,
                    "Falha ao processar a mensagem {Tipo} ({MessageId}) na tentativa {Tentativa}.",
                    recebida.Tipo, recebida.MessageId, recebida.TentativasDeEntrega);
            }
        }

        /// <summary>
        /// Abre o span do consumidor ligado ao trace de quem publicou, quando o
        /// <c>traceparent</c> chega na mensagem.
        /// </summary>
        private static Activity? IniciarAtividade(MensagemRecebida mensagem)
        {
            if (!string.IsNullOrWhiteSpace(mensagem.Traceparent) &&
                ActivityContext.TryParse(mensagem.Traceparent, null, out var contexto))
            {
                return SagaMetrics.Source.StartActivity($"saga receive {mensagem.Tipo}", ActivityKind.Consumer, contexto);
            }

            return SagaMetrics.Source.StartActivity($"saga receive {mensagem.Tipo}", ActivityKind.Consumer);
        }

        private Task ApagarAsync(Message mensagem, CancellationToken cancellationToken) =>
            sqs.DeleteMessageAsync(_opcoes.QueueUrl, mensagem.ReceiptHandle, cancellationToken);
    }
}
