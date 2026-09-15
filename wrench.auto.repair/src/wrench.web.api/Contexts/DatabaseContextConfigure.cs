using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using wrench.auto.repair.autenticacao.infra;
using wrench.auto.repair.autenticacao.infra.Seeds;
using wrench.auto.repair.cadastro.infra;
using wrench.auto.repair.estoque.infra.Context;
using wrench.auto.repair.ordem.servico.infra.Context;
using wrench.auto.repair.ordem.servico.infra.Seeds;
using wrench.web.api.Options;

namespace wrench.web.api.Contexts
{
    public static class DatabaseContextConfigure
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var autenticacaoContext = services.GetRequiredService<AutenticacaoContext>();
                await autenticacaoContext.Database.MigrateAsync();

                var ordemServicoContext = services.GetRequiredService<OrdemServicoDbContext>();
                await ordemServicoContext.Database.MigrateAsync();

                var cadastroContext = services.GetRequiredService<CadastroContext>();
                await cadastroContext.Database.MigrateAsync();

                var estoqueContext = services.GetRequiredService<PecaDbContext>();
                await estoqueContext.Database.MigrateAsync();
            }
        }

        public static async Task UseSeeds(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<AutenticacaoContext>();

            await DbSeeder.SeedAdminAsync(services);
            await OrdemServicoFunctionSeed.CriarFuncaoDateDiffMillisecondsAsync(services);
        }

        public static void AddDbContexts(this IServiceCollection services)
        {
            AddAutenticacaoDbContext(services);
            AddEstoqueDbContext(services);
            AddOrdemServicoDbContext(services);
            AddCadastroDbContext(services);
        }

        private static void AddOrdemServicoDbContext(IServiceCollection services)
        {
            services.AddDbContext<OrdemServicoDbContext>((serviceProvider, dbContextOptionsBuilder) =>
            {
                var databaseOptions = serviceProvider.GetService<IOptions<DatabaseOptions>>()!.Value;

                dbContextOptionsBuilder.UseNpgsql(databaseOptions.ConnectionString, sqlAction =>
                {
                    sqlAction.EnableRetryOnFailure(databaseOptions.MaxRetryCount);
                    sqlAction.CommandTimeout(databaseOptions.CommandTimeout);
                    sqlAction.MigrationsAssembly("wrench.auto.repair.ordem.servico.infra");
                });

                dbContextOptionsBuilder.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
                dbContextOptionsBuilder.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
            });
        }

        private static void AddEstoqueDbContext(IServiceCollection services)
        {
            services.AddDbContext<PecaDbContext>((serviceProvider, dbContextOptionsBuilder) =>
            {
                var databaseOptions = serviceProvider.GetService<IOptions<DatabaseOptions>>()!.Value;

                dbContextOptionsBuilder.UseNpgsql(databaseOptions.ConnectionString, sqlAction =>
                {
                    sqlAction.EnableRetryOnFailure(databaseOptions.MaxRetryCount);
                    sqlAction.CommandTimeout(databaseOptions.CommandTimeout);
                    sqlAction.MigrationsAssembly("wrench.auto.repair.estoque.infra");
                });


                dbContextOptionsBuilder.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
                dbContextOptionsBuilder.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
            });
        }

        private static void AddAutenticacaoDbContext(IServiceCollection services)
        {
            services.AddDbContext<AutenticacaoContext>((serviceProvider, dbContextOptionsBuilder) =>
            {
                var databaseOptions = serviceProvider.GetService<IOptions<DatabaseOptions>>()!.Value;

                dbContextOptionsBuilder.UseNpgsql(databaseOptions.ConnectionString, sqlAction =>
                {
                    sqlAction.EnableRetryOnFailure(databaseOptions.MaxRetryCount);
                    sqlAction.CommandTimeout(databaseOptions.CommandTimeout);
                    sqlAction.MigrationsAssembly("wrench.auto.repair.autenticacao.infra");
                });

                dbContextOptionsBuilder.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
                dbContextOptionsBuilder.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
            });
        }

        private static void AddCadastroDbContext(IServiceCollection services)
        {
            services.AddDbContext<CadastroContext>((serviceProvider, dbContextOptionsBuilder) =>
            {
                var databaseOptions = serviceProvider.GetService<IOptions<DatabaseOptions>>()!.Value;

                dbContextOptionsBuilder.UseNpgsql(databaseOptions.ConnectionString, sqlAction =>
                {
                    sqlAction.EnableRetryOnFailure(databaseOptions.MaxRetryCount);
                    sqlAction.CommandTimeout(databaseOptions.CommandTimeout);
                    sqlAction.MigrationsAssembly("wrench.auto.repair.cadastro.infra");
                });

                dbContextOptionsBuilder.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
                dbContextOptionsBuilder.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
            });
        }
    }
}
