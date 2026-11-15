using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.domain.Saga
{
    public enum ResultadoSaga
    {
        /// <summary>A mensagem mudou o estado da Saga.</summary>
        Processada,

        /// <summary>Mensagem repetida ou fora de ordem: nada a fazer, pode ser removida da fila.</summary>
        Ignorada,

        /// <summary>Não existe Saga para a ordem de serviço informada.</summary>
        SagaNaoEncontrada,

        /// <summary>O tipo da mensagem não é tratado pelo orquestrador.</summary>
        TipoDesconhecido
    }

    public interface ISagaOrquestrador
    {
        /// <summary>
        /// Abre a Saga da ordem de serviço e enfileira <c>GerarOrcamento</c> no outbox. Não
        /// persiste: quem chama confirma a transação junto com a mudança da ordem de serviço.
        /// </summary>
        Task IniciarAsync(OrdemServico ordemServico, string correlationId, CancellationToken cancellationToken);

        /// <summary>Aplica uma mensagem recebida da fila e confirma a transação.</summary>
        Task<ResultadoSaga> ProcessarAsync(MensagemRecebida mensagem, CancellationToken cancellationToken);

        /// <summary>Compensa as Sagas cujo prazo da etapa corrente expirou.</summary>
        Task<int> AplicarTimeoutsAsync(CancellationToken cancellationToken);
    }
}
