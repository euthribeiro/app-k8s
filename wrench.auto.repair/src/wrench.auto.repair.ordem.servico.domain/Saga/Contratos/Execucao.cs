namespace Wrench.Contracts;

/// <summary>Peça da ordem de serviço enviada para a fila de execução.</summary>
public sealed record ItemExecucaoDto(Guid PecaId, string Nome, int Quantidade);

/// <summary>Comando: a ordem de serviço foi paga e entra na fila de execução.</summary>
public sealed record IniciarExecucao(
    Guid OrdemServicoId,
    string CorrelationId,
    string Descricao,
    IReadOnlyList<ItemExecucaoDto> Itens,
    decimal ValorAprovado) : ISagaMensagem;

/// <summary>Compensação: remove a ordem de serviço da fila de execução.</summary>
public sealed record CancelarExecucao(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo) : ISagaMensagem;

/// <summary>Evento: o mecânico concluiu o reparo.</summary>
public sealed record ExecucaoConcluida(
    Guid OrdemServicoId,
    string CorrelationId,
    DateTime ConcluidaEm) : ISagaMensagem;

/// <summary>Evento: a execução não pôde ser realizada e a Saga precisa compensar.</summary>
public sealed record ExecucaoFalhou(
    Guid OrdemServicoId,
    string CorrelationId,
    string Motivo) : ISagaMensagem;
