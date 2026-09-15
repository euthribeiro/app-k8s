using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;

namespace wrench.web.api.Configuration
{
    public static class SerilogConfiguration
    {
        /// <summary>
        /// Logs estruturados em JSON no console (coletados via stdout pelo agente de
        /// Kubernetes/New Relic Infrastructure) e, opcionalmente, exportados também via
        /// OTLP para o New Relic quando OTEL_EXPORTER_OTLP_ENDPOINT estiver definida —
        /// permitindo correlação log/trace pelo mesmo trace.id/span.id.
        /// O SelfLog do Serilog é direcionado ao stderr: sem ele, falhas de sink (por exemplo,
        /// OTLP com credencial inválida) são descartadas em silêncio.
        /// </summary>
        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine($"[Serilog SelfLog] {msg}"));

            var serviceName = builder.Configuration["Observability:ServiceName"] ?? ObservabilityConfiguration.DefaultServiceName;
            var environmentName = builder.Environment.EnvironmentName;

            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithSpan()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty("service.name", serviceName)
                    .Enrich.WithProperty("deployment.environment", environmentName)
                    .WriteTo.Console(new RenderedCompactJsonFormatter());

                var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    var protocol = ResolveProtocol();

                    loggerConfiguration.WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = protocol == OtlpProtocol.HttpProtobuf
                            ? BuildHttpLogsEndpoint(otlpEndpoint)
                            : otlpEndpoint;
                        options.Protocol = protocol;
                        options.Headers = ParseHeaders(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS"));
                        options.ResourceAttributes = new Dictionary<string, object>
                        {
                            ["service.name"] = serviceName,
                            ["deployment.environment"] = environmentName
                        };
                    });
                }
            });
        }

        /// <summary>
        /// Completa o endpoint base com o caminho do sinal de logs. Diferente do exporter do SDK do
        /// OpenTelemetry, o sink do Serilog não deriva <c>/v1/logs</c> do endpoint base, e em HTTP o
        /// caminho precisa ser explícito.
        /// </summary>
        private static string BuildHttpLogsEndpoint(string baseEndpoint)
        {
            var trimmed = baseEndpoint.TrimEnd('/');
            return trimmed.EndsWith("/v1/logs", StringComparison.OrdinalIgnoreCase)
                ? trimmed
                : $"{trimmed}/v1/logs";
        }

        private static OtlpProtocol ResolveProtocol()
        {
            var protocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL");
            return string.Equals(protocol, "grpc", StringComparison.OrdinalIgnoreCase)
                ? OtlpProtocol.Grpc
                : OtlpProtocol.HttpProtobuf;
        }

        private static IDictionary<string, string> ParseHeaders(string? rawHeaders)
        {
            var headers = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(rawHeaders))
            {
                return headers;
            }

            foreach (var pair in rawHeaders.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                {
                    headers[parts[0].Trim()] = Uri.UnescapeDataString(parts[1].Trim());
                }
            }

            return headers;
        }
    }
}
