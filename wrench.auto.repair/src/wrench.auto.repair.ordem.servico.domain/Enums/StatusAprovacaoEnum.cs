using System.ComponentModel;

namespace wrench.auto.repair.ordem.servico.domain.Enums
{
    public enum StatusAprovacaoEnum
    {
        [Description("Indefinido")]
        Indefinido = 0,

        [Description("Em análise")]
        EmAnalise = 1,

        [Description("Aprovada")]
        Aprovada = 2,

        [Description("Recusada")]
        Recusada = 3
    }
}
