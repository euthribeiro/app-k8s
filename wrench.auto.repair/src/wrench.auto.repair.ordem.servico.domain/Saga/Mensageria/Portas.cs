using Wrench.Contracts;

namespace wrench.auto.repair.ordem.servico.domain.Saga.Mensageria
{
    /// <summary>
    /// Outbox transacional: o orquestrador enfileira a mensagem no mesmo <c>DbContext</c> que
    /// grava o novo estado, de modo que ou os dois são persistidos ou nenhum dos dois.
    /// </summary>
    public interface IOutboxSaga
    {
        void Enfileirar<TContrato>(string tipo, TContrato conteudo) where TContrato : ISagaMensagem;

        Task<IReadOnlyList<MensagemSaga>> ObterPendentesAsync(int limite, CancellationToken cancellationToken);

        Task MarcarPublicadaAsync(Guid id, DateTime publicadaEm, CancellationToken cancellationToken);

        Task<int> ContarPendentesAsync(CancellationToken cancellationToken);
    }

    /// <summary>Registro das mensagens já consumidas, que torna o consumidor idempotente.</summary>
    public interface IInboxSaga
    {
        Task<bool> JaProcessadaAsync(string messageId, CancellationToken cancellationToken);

        void Registrar(string messageId, string tipo, DateTime recebidaEm);
    }

    /// <summary>Publicação da mensagem no transporte (SNS).</summary>
    public interface IPublicadorSaga
    {
        Task PublicarAsync(MensagemSaga mensagem, CancellationToken cancellationToken);
    }
}
