namespace wrench.auto.repair.core.Messages.CommonMessages.IntegrationEvents
{
    public class ClienteCadastradoEvent(Guid clienteId, string email) : IntegrationEvent
    {
        public Guid ClienteId { get; private set; } = clienteId;
        public string Email { get; private set; } = email;
    }
}
