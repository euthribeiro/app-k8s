namespace Wrench.Contracts;

/// <summary>
/// Nomes dos message attributes que acompanham toda mensagem publicada no SNS.
/// O corpo da mensagem é o JSON do contrato (raw message delivery).
/// </summary>
public static class AtributosMensagem
{
    public const string MessageId = "messageId";
    public const string Tipo = "tipo";
    public const string CorrelationId = "correlationId";
    public const string Traceparent = "traceparent";
}
