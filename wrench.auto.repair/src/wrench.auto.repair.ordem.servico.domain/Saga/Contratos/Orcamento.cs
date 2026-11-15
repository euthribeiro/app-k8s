namespace Wrench.Contracts;

/// <summary>Peça do orçamento enviada do OS Service para o Billing Service.</summary>
public sealed record ItemOrcamentoDto(Guid PecaId, string Nome, int Quantidade, decimal ValorUnitario);

/// <summary>Comando: o diagnóstico foi concluído e o Billing deve montar o orçamento.</summary>
public sealed record GerarOrcamento(
    Guid OrdemServicoId,
    string CorrelationId,
    Guid ClienteId,
    string Descricao,
    string SolucaoProposta,
    decimal ValorServico,
    IReadOnlyList<ItemOrcamentoDto> Itens) : ISagaMensagem;

/// <summary>Compensação: cancela o orçamento aberto para a ordem de serviço.</summary>
public sealed record CancelarOrcamento(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo) : ISagaMensagem;

/// <summary>Evento: o orçamento foi criado e enviado ao cliente.</summary>
public sealed record OrcamentoCriado(
    Guid OrdemServicoId,
    string CorrelationId,
    Guid OrcamentoId,
    decimal ValorTotal,
    DateTime CriadoEm) : ISagaMensagem;

/// <summary>Evento: o Billing não conseguiu montar o orçamento.</summary>
public sealed record OrcamentoFalhou(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo) : ISagaMensagem;

/// <summary>Evento: o cliente aprovou o orçamento; a cobrança será criada.</summary>
public sealed record OrcamentoAprovado(
    Guid OrdemServicoId,
    string CorrelationId,
    Guid OrcamentoId,
    decimal ValorAprovado,
    DateTime AprovadoEm) : ISagaMensagem;

/// <summary>Evento: o cliente recusou o orçamento.</summary>
public sealed record OrcamentoRecusado(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo,
    DateTime RecusadoEm) : ISagaMensagem;
