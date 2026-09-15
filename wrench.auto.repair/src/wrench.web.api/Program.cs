using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using wrench.auto.repair.autenticacao.application.Extensions;
using wrench.auto.repair.autenticacao.infra;
using wrench.auto.repair.autenticacao.infra.Extensions;
using wrench.auto.repair.cadastro.application.Extensions;
using wrench.auto.repair.cadastro.infra;
using wrench.auto.repair.cadastro.infra.Extensions;
using wrench.auto.repair.core.Extensions;
using wrench.auto.repair.core.Security;
using wrench.auto.repair.estoque.application.Extensions;
using wrench.auto.repair.estoque.infra.Context;
using wrench.auto.repair.estoque.infra.Extensions;
using wrench.auto.repair.infra.Extensions;
using wrench.auto.repair.ordem.servico.application.Extensions;
using wrench.auto.repair.ordem.servico.infra.Context;
using wrench.auto.repair.ordem.servico.infra.Extensions;
using wrench.web.api.Configuration;
using wrench.web.api.Contexts;
using wrench.web.api.Docs;
using wrench.web.api.HealthCheck;
using wrench.web.api.Middlewares;
using wrench.web.api.Options;
using wrench.web.api.Transformers;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();

builder.Services.AddProblemDetails();

builder.Services.AddObservability(builder.Configuration, builder.Environment);

builder.Services.ConfigureOptions<DatabaseOptionsSetup>();
builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<EmailOptionsSetup>();
builder.Services.ConfigureAuthentication(builder.Configuration);

builder.Services.AddDbContexts();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<AutenticacaoContext>(tags: ["ready"])
    .AddDbContextCheck<OrdemServicoDbContext>(tags: ["ready"])
    .AddDbContextCheck<PecaDbContext>(tags: ["ready"])
    .AddDbContextCheck<CadastroContext>(tags: ["ready"])
    .AddCheck<DatabaseMigrationHealthCheck<AutenticacaoContext>>(name: "autenticacao_context_migrations", tags: ["ready"])
    .AddCheck<DatabaseMigrationHealthCheck<OrdemServicoDbContext>>(name: "ordem_servico_context_migrations", tags: ["ready"])
    .AddCheck<DatabaseMigrationHealthCheck<PecaDbContext>>(name: "peca_db_context_migrations", tags: ["ready"])
    .AddCheck<DatabaseMigrationHealthCheck<CadastroContext>>(name: "cadastro_context_migrations", tags: ["ready"]);

builder.Services
    .AddCore()
    .AddAutenticacaoApplication()
    .AddAutenticacaoInfra()
    .AddCadastroApplication()
    .AddCadastroInfra()
    .AddEstoqueApplication()
    .AddEstoqueInfra()
    .AddOrdemServicoApplication()
    .AddOrdemServicoInfra()
    .AddWrenchInfra(builder.Configuration);

builder.Services.ConfigureOpenApi();

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(
        new RouteTokenTransformerConvention(
            new SlugifyParameterTransformer()));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new SensitiveDataConverter());
});

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.DefaultApiVersion = new ApiVersion(1.0);
    setupAction.ReportApiVersions = true;
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

await app.ApplyMigrationsAsync();
await app.UseSeeds();

app.UseCorrelationId();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseGlobalExceptionHandler();
}

app.UseForwardedHeaders();

app.MapOpenApi();

app.MapScalarApiReference("docs-ui", options =>
{
    options.Title = "Wrench API";
});

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", (HttpContext context) => Results.Redirect("/docs-ui", permanent: true));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    AllowCachingResponses = false,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();