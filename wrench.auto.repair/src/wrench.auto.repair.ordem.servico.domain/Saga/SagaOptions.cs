namespace wrench.auto.repair.ordem.servico.domain.Saga
{
    /// <summary>
    /// Prazos de cada etapa da Saga. O estouro do prazo dispara a mesma compensação
    /// da falha correspondente.
    /// </summary>
    public class SagaOptions
    {
        public const string Secao = "Saga";

        public int PrazoOrcamentoMinutos { get; set; } = 15;

        public int PrazoAprovacaoMinutos { get; set; } = 2880;

        public int PrazoPagamentoMinutos { get; set; } = 60;

        public int PrazoExecucaoMinutos { get; set; } = 2880;

        public int PrazoEstornoMinutos { get; set; } = 60;

        public int IntervaloVerificacaoSegundos { get; set; } = 30;

        public int LoteTimeouts { get; set; } = 25;

        public TimeSpan PrazoOrcamento => TimeSpan.FromMinutes(PrazoOrcamentoMinutos);
        public TimeSpan PrazoAprovacao => TimeSpan.FromMinutes(PrazoAprovacaoMinutos);
        public TimeSpan PrazoPagamento => TimeSpan.FromMinutes(PrazoPagamentoMinutos);
        public TimeSpan PrazoExecucao => TimeSpan.FromMinutes(PrazoExecucaoMinutos);
        public TimeSpan PrazoEstorno => TimeSpan.FromMinutes(PrazoEstornoMinutos);
    }
}
