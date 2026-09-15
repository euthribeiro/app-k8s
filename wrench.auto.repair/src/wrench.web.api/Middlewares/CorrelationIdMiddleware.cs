using System.Diagnostics;
using Serilog.Context;

namespace wrench.web.api.Middlewares
{
    /// <summary>
    /// Garante um identificador de correlação de negócio por requisição. Reaproveita o valor do header
    /// <c>X-Correlation-ID</c> quando enviado, ou gera um novo; devolve o valor no mesmo header da
    /// resposta, grava-o como tag <c>correlation_id</c> da <see cref="Activity"/> corrente (ligando o
    /// trace do OpenTelemetry ao identificador de negócio) e como propriedade <c>CorrelationId</c> de
    /// todos os logs emitidos durante a requisição.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = ResolveCorrelationId(context);

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            Activity.Current?.SetTag("correlation_id", correlationId);

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderName, out var existing) &&
                !string.IsNullOrWhiteSpace(existing))
            {
                return existing.ToString();
            }

            return Guid.NewGuid().ToString();
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
