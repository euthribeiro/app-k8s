using System.Diagnostics;
using System.Text.Json;
using Wrench.Contracts;

namespace wrench.auto.repair.ordem.servico.domain.Saga.Mensageria
{
    /// <summary>
    /// Mensagem de saída gravada no outbox na mesma transação da mudança de estado.
    /// O dispatcher a publica no SNS e marca como publicada.
    /// </summary>
    public sealed record MensagemSaga(
        Guid Id,
        Guid OrdemServicoId,
        string Tipo,
        string Payload,
        string CorrelationId,
        string? Traceparent,
        DateTime CriadaEm)
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        public static MensagemSaga Criar<TContrato>(string tipo, TContrato conteudo, DateTime agora)
            where TContrato : ISagaMensagem =>
            new(Guid.NewGuid(),
                conteudo.OrdemServicoId,
                tipo,
                JsonSerializer.Serialize(conteudo, Json),
                conteudo.CorrelationId,
                Activity.Current?.Id,
                agora);
    }

    /// <summary>Mensagem recebida da fila, já com os metadados lidos dos message attributes.</summary>
    public sealed record MensagemRecebida(
        string MessageId,
        string Tipo,
        string Corpo,
        string? CorrelationId,
        string? Traceparent,
        int TentativasDeEntrega);
}
