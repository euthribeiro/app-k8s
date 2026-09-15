using wrench.auto.repair.core.Messages;

namespace wrench.auto.repair.ordem.servico.application.Events
{
    public class OrdemServicoAguardandoAprovacaoEvent(Guid ordemServicoId) : Event
    {
        public Guid OrdemServicoId { get; private set; } = ordemServicoId;
    }
}
