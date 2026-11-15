using System.ComponentModel;

namespace wrench.auto.repair.ordem.servico.domain.Saga
{
    /// <summary>Etapa da Saga em andamento; identifica qual compensação aplicar em caso de falha.</summary>
    public enum SagaEtapa
    {
        [Description("Nenhuma")]
        Nenhuma = 0,

        [Description("Geração do orçamento")]
        Orcamento = 1,

        [Description("Aprovação do cliente")]
        Aprovacao = 2,

        [Description("Pagamento")]
        Pagamento = 3,

        [Description("Execução")]
        Execucao = 4,

        [Description("Estorno")]
        Estorno = 5
    }
}
