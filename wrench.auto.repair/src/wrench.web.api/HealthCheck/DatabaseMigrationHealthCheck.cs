using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace wrench.web.api.HealthCheck
{
    /// <summary>
    /// Verifica se o contexto <typeparamref name="TContext"/> esta com todas as migrations
    /// aplicadas. Registrado com a tag <c>ready</c>, e o que impede um pod com schema
    /// desatualizado de entrar no balanceador.
    /// </summary>
    /// <typeparam name="TContext">Contexto de persistencia auditado.</typeparam>
    public class DatabaseMigrationHealthCheck<TContext> : IHealthCheck where TContext : DbContext
    {
        private readonly TContext _dbContext;

        /// <param name="dbContext">Contexto cujo estado de migration sera verificado.</param>
        public DatabaseMigrationHealthCheck(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Reporta <c>Unhealthy</c> quando existem migrations pendentes — listando quais — ou
        /// quando a consulta ao banco falha; caso contrario, <c>Healthy</c>.
        /// </summary>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var pendingMigrations = await _dbContext.Database
                    .GetPendingMigrationsAsync(cancellationToken);

                var pendingList = pendingMigrations.ToList();

                if (pendingList.Any())
                {
                    return HealthCheckResult.Unhealthy(
                        description: $"Database migration mismatch. Pending migrations: {string.Join(", ", pendingList)}");
                }

                return HealthCheckResult.Healthy("All database migrations are applied.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    description: "Failed to check database migration status.",
                    exception: ex);
            }
        }
    }
}
