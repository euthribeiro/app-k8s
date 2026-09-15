using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using wrench.web.api.Middlewares;

namespace wrench.web.api.tests
{
    public class CorrelationIdMiddlewareTests
    {
        private sealed class CollectingSink : ILogEventSink
        {
            public List<LogEvent> Events { get; } = [];

            public void Emit(LogEvent logEvent) => Events.Add(logEvent);
        }

        private static TestServer CreateServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => services.AddRouting())
                .Configure(app =>
                {
                    app.UseCorrelationId();
                    app.Run(context =>
                    {
                        Log.Information("inside pipeline");
                        return context.Response.WriteAsync("ok");
                    });
                });

            return new TestServer(builder);
        }

        [Fact]
        public async Task Request_GeneratesCorrelationId_AndPushesItToLogContext_WhenHeaderIsAbsent()
        {
            var sink = new CollectingSink();
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Sink(sink)
                .CreateLogger();

            using var server = CreateServer();
            using var client = server.CreateClient();

            var response = await client.GetAsync("/");

            Assert.True(response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values));
            var correlationId = Assert.Single(values!);
            Assert.True(Guid.TryParse(correlationId, out _));

            var logged = Assert.Single(sink.Events);
            Assert.Equal($"\"{correlationId}\"", logged.Properties["CorrelationId"].ToString());

            Log.CloseAndFlush();
        }

        [Fact]
        public async Task Request_ReusesExistingCorrelationId_WhenHeaderIsPresent()
        {
            const string existingId = "meu-correlation-id-123";

            using var server = CreateServer();
            using var client = server.CreateClient();
            client.DefaultRequestHeaders.Add(CorrelationIdMiddleware.HeaderName, existingId);

            var response = await client.GetAsync("/");

            Assert.True(response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values));
            Assert.Equal(existingId, Assert.Single(values!));
        }
    }
}
