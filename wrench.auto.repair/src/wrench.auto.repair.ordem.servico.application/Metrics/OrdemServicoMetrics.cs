using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using wrench.auto.repair.ordem.servico.domain.Data;

namespace wrench.auto.repair.ordem.servico.application.Metrics
{
    /// <summary>
    /// Métricas de negócio da Ordem de Serviço publicadas no meter <see cref="MeterName"/>.
    /// Os tempos médios por fase são recalculados a cada 5 minutos a partir de agregações no banco
    /// (ver <c>OrdemServicoRepository</c>) e expostos como gauges. O <see cref="ObservableGauge{T}"/> não aceita
    /// callback assíncrono, por isso os valores ficam em cache e são atualizados em background; uma falha na
    /// atualização é registrada em log e mantém o último valor conhecido, sem derrubar o host.
    /// O contador <see cref="FalhasProcessamentoNome"/> é alimentado por <see cref="FalhaProcessamentoOrdemServicoBehavior{TRequest, TResponse}"/>.
    /// </summary>
    public sealed class OrdemServicoMetrics : BackgroundService
    {
        public const string MeterName = "Wrench.OrdemServico";

        /// <summary>Nome do contador de comandos de ordem de serviço que terminaram em falha inesperada.</summary>
        public const string FalhasProcessamentoNome = "ordemservico.processamento.falhas";

        /// <summary>Atributo do contador de falhas com o nome do comando, por exemplo <c>FinalizarOrdemServicoCommand</c>.</summary>
        public const string AtributoComando = "comando";

        /// <summary>Atributo do contador de falhas com o tipo do erro: nome da exceção ou <c>INESPERADO</c>.</summary>
        public const string AtributoTipoErro = "tipo_erro";

        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);
        private static readonly Meter Meter = new(MeterName);

        private static readonly Counter<long> FalhasProcessamento = Meter.CreateCounter<long>(
            FalhasProcessamentoNome,
            unit: "{falha}",
            description: "Comandos de ordem de servico que terminaram em excecao ou erro inesperado");

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrdemServicoMetrics> _logger;

        private double _tempoMedioDiagnosticoMs;
        private double _tempoMedioExecucaoMs;
        private double _tempoMedioEntregaMs;

        public OrdemServicoMetrics(IServiceScopeFactory scopeFactory, ILogger<OrdemServicoMetrics> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            Meter.CreateObservableGauge(
                "ordemservico.diagnostico.duration",
                () => _tempoMedioDiagnosticoMs,
                unit: "ms",
                description: "Tempo medio da criacao da OS ate a conclusao do diagnostico");

            Meter.CreateObservableGauge(
                "ordemservico.execucao.duration",
                () => _tempoMedioExecucaoMs,
                unit: "ms",
                description: "Tempo medio entre aprovacao do orcamento e finalizacao da OS");

            Meter.CreateObservableGauge(
                "ordemservico.entrega.duration",
                () => _tempoMedioEntregaMs,
                unit: "ms",
                description: "Tempo medio entre finalizacao e entrega da OS");
        }

        /// <summary>Incrementa em uma unidade o contador <see cref="FalhasProcessamentoNome"/>.</summary>
        /// <param name="comando">Nome do tipo do comando que falhou.</param>
        /// <param name="tipoErro">Nome da exceção lançada ou <c>INESPERADO</c> quando o handler devolveu esse tipo de erro.</param>
        public static void RegistrarFalhaProcessamento(string comando, string tipoErro)
        {
            FalhasProcessamento.Add(
                1,
                new KeyValuePair<string, object?>(AtributoComando, comando),
                new KeyValuePair<string, object?>(AtributoTipoErro, tipoErro));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await AtualizarAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao atualizar as metricas de tempo por fase da Ordem de Servico");
                }

                try
                {
                    await Task.Delay(Intervalo, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task AtualizarAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var repositorio = scope.ServiceProvider.GetRequiredService<IOrdemServicoRepository>();

            _tempoMedioDiagnosticoMs = await repositorio.ObterTempoMedioDiagnostico();
            _tempoMedioExecucaoMs = await repositorio.ObterTempoMedioExecucao();
            _tempoMedioEntregaMs = await repositorio.ObterTempoMedioEntrega();
        }
    }
}
