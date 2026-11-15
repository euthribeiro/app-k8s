using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using Wrench.Contracts;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.infra.Saga.Mensageria
{
    /// <summary>
    /// Publica a mensagem no tópico do OS Service. O corpo é o JSON do contrato e os metadados
    /// vão como message attributes: o <c>tipo</c> é o que o filter policy da assinatura avalia e
    /// o <c>traceparent</c> mantém o trace distribuído ligado no New Relic.
    /// </summary>
    public class SnsPublicadorSaga(IAmazonSimpleNotificationService sns, IOptions<SnsOptions> opcoes) : IPublicadorSaga
    {
        private readonly SnsOptions _opcoes = opcoes.Value;

        public async Task PublicarAsync(MensagemSaga mensagem, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_opcoes.TopicArn))
                throw new InvalidOperationException("O topico SNS (Sns:TopicArn) nao foi configurado.");

            var requisicao = new PublishRequest
            {
                TopicArn = _opcoes.TopicArn,
                Message = mensagem.Payload,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    [AtributosMensagem.MessageId] = Atributo(mensagem.Id.ToString()),
                    [AtributosMensagem.Tipo] = Atributo(mensagem.Tipo),
                    [AtributosMensagem.CorrelationId] = Atributo(mensagem.CorrelationId)
                }
            };

            if (!string.IsNullOrEmpty(mensagem.Traceparent))
                requisicao.MessageAttributes[AtributosMensagem.Traceparent] = Atributo(mensagem.Traceparent);

            await sns.PublishAsync(requisicao, cancellationToken);
        }

        private static MessageAttributeValue Atributo(string valor) =>
            new() { DataType = "String", StringValue = valor };
    }
}
