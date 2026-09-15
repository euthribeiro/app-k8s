using wrench.auto.repair.core.Messages;
using wrench.auto.repair.ordem.servico.domain.Enums;

namespace wrench.auto.repair.ordem.servico.application.Events
{
    public class OrdemServicoAtualizadaEvent(Guid ordemServicoId, Guid clienteId, OrdemServicoStatus novoStatus) : Event
    {
        public Guid OrdemServicoId { get; } = ordemServicoId;
        public Guid ClienteId { get; } = clienteId;
        public OrdemServicoStatus NovoStatus { get; } = novoStatus;
    }
}
