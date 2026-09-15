
using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.infra.Context;

public class PecaDbContext : DbContext, IUnitOfWork
{
    public DbSet<Peca> Pecas { get; set; }

    public PecaDbContext(DbContextOptions<PecaDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PecaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public async Task<bool> CommitAsync()
    {
        return await base.SaveChangesAsync() > 0;
    }
}