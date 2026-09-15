using System.ComponentModel;

namespace wrench.auto.repair.ordem.servico.domain.Enums
{
    public enum OrdemServicoStatus
    {
        [Description("Recebida")]
        Recebida = 1,

        [Description("Em diagnóstico")]
        EmDiagnostico = 2,

        [Description("Aguardando aprovação")]
        AguardandoAprovacao = 3,

        [Description("Em execução")]
        EmExecucao = 4,

        [Description("Finalizada")]
        Finalizada = 5,

        [Description("Entregue")]
        Entregue = 6
    }
}
