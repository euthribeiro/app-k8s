using Amazon.SQS.Model;
using Wrench.Contracts;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.infra.Saga.Mensageria
{
    /// <summary>
    /// Traduz a mensagem bruta do SQS para o formato que o orquestrador entende. Com
    /// <c>raw message delivery</c> o corpo é o próprio JSON do contrato e os metadados chegam
    /// como message attributes; sem o atributo <c>tipo</c> não há como rotear a mensagem.
    /// </summary>
    public static class ConversorMensagemSqs
    {
        public const string ContagemDeEntregas = "ApproximateReceiveCount";

        public static MensagemRecebida? Converter(Message mensagem)
        {
            var atributos = mensagem.MessageAttributes;

            if (atributos is null ||
                !atributos.TryGetValue(AtributosMensagem.Tipo, out var tipo) ||
                string.IsNullOrWhiteSpace(tipo.StringValue))
            {
                return null;
            }

            atributos.TryGetValue(AtributosMensagem.MessageId, out var messageId);
            atributos.TryGetValue(AtributosMensagem.CorrelationId, out var correlationId);
            atributos.TryGetValue(AtributosMensagem.Traceparent, out var traceparent);

            return new MensagemRecebida(
                messageId?.StringValue ?? mensagem.MessageId,
                tipo.StringValue,
                mensagem.Body,
                correlationId?.StringValue,
                traceparent?.StringValue,
                LerTentativas(mensagem));
        }

        private static int LerTentativas(Message mensagem)
        {
            if (mensagem.Attributes is not null &&
                mensagem.Attributes.TryGetValue(ContagemDeEntregas, out var contagem) &&
                int.TryParse(contagem, out var entregas) &&
                entregas > 0)
            {
                return entregas;
            }

            return 1;
        }
    }
}
