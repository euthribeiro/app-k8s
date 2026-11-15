using Microsoft.Extensions.Logging;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.infra.Saga.Mensageria
{
    /// <summary>
    /// Publica um lote de mensagens pendentes do outbox. Uma falha isolada não interrompe o
    /// lote nem marca a mensagem como publicada: ela continua pendente e volta na próxima
    /// rodada, o que garante entrega ao menos uma vez.
    /// </summary>
    public class PublicacaoOutbox(
        IOutboxSaga outbox,
        IPublicadorSaga publicador,
        TimeProvider relogio,
        ILogger<PublicacaoOutbox> logger)
    {
        public int Pendentes { get; private set; }

        public async Task<int> PublicarPendentesAsync(int tamanhoLote, CancellationToken cancellationToken)
        {
            var pendentes = await outbox.ObterPendentesAsync(tamanhoLote, cancellationToken);

            Pendentes = await outbox.ContarPendentesAsync(cancellationToken);

            var publicadas = 0;

            foreach (var mensagem in pendentes)
            {
                try
                {
                    await publicador.PublicarAsync(mensagem, cancellationToken);

                    await outbox.MarcarPublicadaAsync(mensagem.Id, relogio.GetUtcNow().UtcDateTime, cancellationToken);

                    SagaMetrics.MensagemPublicada(mensagem.Tipo);

                    publicadas++;
                }
                catch (Exception excecao)
                {
                    logger.LogError(excecao,
                        "Falha ao publicar a mensagem {MessageId} ({Tipo}) da ordem de servico {OrdemServicoId}; sera tentada de novo.",
                        mensagem.Id, mensagem.Tipo, mensagem.OrdemServicoId);
                }
            }

            return publicadas;
        }
    }
}
