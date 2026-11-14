namespace Wrench.Contracts;

/// <summary>
/// Valor do atributo de mensagem <c>tipo</c>, usado pelo filtro das assinaturas SNS
/// e pelo roteamento do consumidor. Mudanças são apenas aditivas.
/// </summary>
public static class MensagemTipos
{
    public const string GerarOrcamento = nameof(Wrench.Contracts.GerarOrcamento);
    public const string CancelarOrcamento = nameof(Wrench.Contracts.CancelarOrcamento);
    public const string EstornarPagamento = nameof(Wrench.Contracts.EstornarPagamento);

    public const string OrcamentoCriado = nameof(Wrench.Contracts.OrcamentoCriado);
    public const string OrcamentoFalhou = nameof(Wrench.Contracts.OrcamentoFalhou);
    public const string OrcamentoAprovado = nameof(Wrench.Contracts.OrcamentoAprovado);
    public const string OrcamentoRecusado = nameof(Wrench.Contracts.OrcamentoRecusado);

    public const string PagamentoAprovado = nameof(Wrench.Contracts.PagamentoAprovado);
    public const string PagamentoFalhou = nameof(Wrench.Contracts.PagamentoFalhou);
    public const string PagamentoEstornado = nameof(Wrench.Contracts.PagamentoEstornado);
    public const string EstornoFalhou = nameof(Wrench.Contracts.EstornoFalhou);

    public const string IniciarExecucao = nameof(Wrench.Contracts.IniciarExecucao);
    public const string CancelarExecucao = nameof(Wrench.Contracts.CancelarExecucao);

    public const string ExecucaoConcluida = nameof(Wrench.Contracts.ExecucaoConcluida);
    public const string ExecucaoFalhou = nameof(Wrench.Contracts.ExecucaoFalhou);

    /// <summary>Tipos publicados pelo OS Service no tópico wrench-os-events.</summary>
    public static readonly IReadOnlyList<string> ComandosDoOrquestrador =
    [
        GerarOrcamento,
        CancelarOrcamento,
        EstornarPagamento,
        IniciarExecucao,
        CancelarExecucao
    ];

    /// <summary>Tipos consumidos pelo OS Service na fila wrench-os-saga.</summary>
    public static readonly IReadOnlyList<string> EventosDoOrquestrador =
    [
        OrcamentoCriado,
        OrcamentoFalhou,
        OrcamentoAprovado,
        OrcamentoRecusado,
        PagamentoAprovado,
        PagamentoFalhou,
        PagamentoEstornado,
        EstornoFalhou,
        ExecucaoConcluida,
        ExecucaoFalhou
    ];
}
