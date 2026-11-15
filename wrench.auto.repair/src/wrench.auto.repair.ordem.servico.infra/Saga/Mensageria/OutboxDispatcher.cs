using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using wrench.auto.repair.ordem.servico.domain.Saga;

namespace wrench.auto.repair.ordem.servico.infra.Saga.Mensageria
{
    /// <summary>
    /// Dá o ritmo da publicação do outbox: a cada intervalo abre um escopo e manda
    /// <see cref="PublicacaoOutbox"/> esvaziar um lote. O gauge de mensagens pendentes
    /// alimenta o alerta de outbox represado no New Relic.
    /// </summary>
    public class OutboxDispatcher(
        IServiceScopeFactory escopos,
        IOptions<OutboxOptions> opcoes,
        ILogger<OutboxDispatcher> logger) : BackgroundService
    {
        private readonly OutboxOptions _opcoes = opcoes.Value;

        private int _pendentes;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SagaMetrics.RegistrarOutboxPendentes(() => _pendentes);

            var intervalo = TimeSpan.FromSeconds(Math.Max(1, _opcoes.IntervaloSegundos));

            using var cadencia = new PeriodicTimer(intervalo);

            while (await cadencia.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var escopo = escopos.CreateScope();

                    var publicacao = escopo.ServiceProvider.GetRequiredService<PublicacaoOutbox>();

                    await publicacao.PublicarPendentesAsync(_opcoes.TamanhoLote, stoppingToken);

                    _pendentes = publicacao.Pendentes;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception excecao)
                {
                    logger.LogError(excecao, "Falha ao publicar o lote do outbox da Saga.");
                }
            }
        }
    }
}
