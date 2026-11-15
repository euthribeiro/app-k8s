using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using wrench.auto.repair.ordem.servico.domain.Saga;

namespace wrench.auto.repair.ordem.servico.infra.Saga.Mensageria
{
    /// <summary>
    /// Varre periodicamente as Sagas cujo prazo da etapa corrente venceu. Um serviço que nunca
    /// responde não pode deixar a ordem de serviço presa: o timeout dispara a mesma compensação
    /// que a falha equivalente teria disparado.
    /// </summary>
    public class SagaTimeoutWorker(
        IServiceScopeFactory escopos,
        IOptions<SagaOptions> opcoes,
        ILogger<SagaTimeoutWorker> logger) : BackgroundService
    {
        private readonly SagaOptions _opcoes = opcoes.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalo = TimeSpan.FromSeconds(Math.Max(5, _opcoes.IntervaloVerificacaoSegundos));

            using var cadencia = new PeriodicTimer(intervalo);

            while (await cadencia.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var escopo = escopos.CreateScope();

                    var orquestrador = escopo.ServiceProvider.GetRequiredService<ISagaOrquestrador>();

                    var compensadas = await orquestrador.AplicarTimeoutsAsync(stoppingToken);

                    if (compensadas > 0)
                        logger.LogWarning("{Total} Saga(s) compensada(s) por timeout.", compensadas);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception excecao)
                {
                    logger.LogError(excecao, "Falha ao aplicar os timeouts da Saga.");
                }
            }
        }
    }
}
