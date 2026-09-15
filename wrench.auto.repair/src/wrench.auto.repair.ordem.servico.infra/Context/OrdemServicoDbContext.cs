using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.ordem.servico.domain.Entities;
using wrench.auto.repair.ordem.servico.infra.Extensions;

namespace wrench.auto.repair.ordem.servico.infra.Context
{
    /// <summary>
    /// Contexto de persistencia das ordens de servico. Registra a funcao de banco
    /// <c>datediff_milliseconds</c>, que permite calcular o tempo de execucao de uma ordem
    /// diretamente em SQL.
    /// </summary>
    /// <remarks>
    /// Os comandos de criacao e aplicacao de migration deste contexto estao em
    /// <c>docs/database/migrations.md</c>.
    /// </remarks>
    public class OrdemServicoDbContext(DbContextOptions<OrdemServicoDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<OrdemServico> OrdemServico { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdemServicoDbContext).Assembly);

            modelBuilder
                .HasDbFunction(typeof(PostgresDbFunctions)
                .GetMethod(nameof(PostgresDbFunctions.DateDiffMilliseconds))!)
                .HasName("datediff_milliseconds");

            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> CommitAsync()
        {
            return await base.SaveChangesAsync() > 0;
        }
    }
}
