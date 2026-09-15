using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using wrench.auto.repair.ordem.servico.application.Metrics;

namespace wrench.web.api.Configuration
{
    public static class ObservabilityConfiguration
    {
        public const string DefaultServiceName = "wrench-auto-repair-api";

        /// <summary>
        /// Traces e métricas via OpenTelemetry. O destino (New Relic OTLP) e as credenciais
        /// não são configurados aqui: o exporter OTLP lê automaticamente as variáveis de
        /// ambiente padrão OTEL_EXPORTER_OTLP_ENDPOINT / OTEL_EXPORTER_OTLP_HEADERS / OTEL_EXPORTER_OTLP_PROTOCOL,
        /// definidas via Kubernetes Secret / env do container. Sem essas variáveis, o SDK
        /// simplesmente não exporta nada (não quebra a aplicação).
        /// As métricas são exportadas com temporalidade delta, a forma nativa de ingestão do New Relic;
        /// contadores como <c>ordemservico.processamento.falhas</c> ficam consultáveis diretamente com <c>sum()</c>.
        /// </summary>
        public static IServiceCollection AddObservability(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var serviceName = configuration["Observability:ServiceName"] ?? DefaultServiceName;
            var serviceVersion = typeof(ObservabilityConfiguration).Assembly.GetName().Version?.ToString() ?? "0.0.0";

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                    .AddAttributes(
                    [
                        new KeyValuePair<string, object>("deployment.environment", environment.EnvironmentName)
                    ]))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = httpContext =>
                            !httpContext.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter())
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(OrdemServicoMetrics.MeterName)
                    .AddOtlpExporter((_, metricReaderOptions) =>
                        metricReaderOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta));

            return services;
        }
    }
}
