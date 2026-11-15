using System.ComponentModel;

namespace wrench.auto.repair.ordem.servico.domain.Saga
{
    /// <summary>Estado corrente da Saga de uma ordem de serviço.</summary>
    public enum SagaEstado
    {
        [Description("Aguardando orçamento")]
        AguardandoOrcamento = 1,

        [Description("Aguardando aprovação do cliente")]
        AguardandoAprovacao = 2,

        [Description("Aguardando pagamento")]
        AguardandoPagamento = 3,

        [Description("Aguardando execução")]
        AguardandoExecucao = 4,

        [Description("Concluída")]
        Concluida = 5,

        [Description("Compensando")]
        Compensando = 6,

        [Description("Compensada")]
        Compensada = 7,

        [Description("Falha na compensação")]
        Falha = 8
    }
}
