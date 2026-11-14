namespace Wrench.Contracts;

/// <summary>Compensação: devolve ao cliente o valor já pago no Mercado Pago.</summary>
public sealed record EstornarPagamento(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo) : ISagaMensagem;

/// <summary>Evento: o webhook do Mercado Pago confirmou o pagamento.</summary>
public sealed record PagamentoAprovado(
    Guid OrdemServicoId,
    string CorrelationId,
    Guid PagamentoId,
    decimal Valor,
    DateTime AprovadoEm) : ISagaMensagem;

/// <summary>Evento: o pagamento foi recusado ou expirou.</summary>
public sealed record PagamentoFalhou(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo) : ISagaMensagem;

/// <summary>Evento: o estorno foi concluído no Mercado Pago.</summary>
public sealed record PagamentoEstornado(
    Guid OrdemServicoId,
    string CorrelationId,
    Guid PagamentoId,
    decimal Valor,
    DateTime EstornadoEm) : ISagaMensagem;

/// <summary>Evento: o estorno não pôde ser concluído e exige tratamento manual.</summary>
public sealed record EstornoFalhou(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo) : ISagaMensagem;
