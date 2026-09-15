using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.cadastro.infra
{
    public class CadastroContext(
        DbContextOptions<CadastroContext> options
    ) : DbContext(options), IUnitOfWork
    {
        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Veiculo> Veiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .Navigation(c => c.Endereco)
                .AutoInclude();

            modelBuilder.Entity<Cliente>()
                .Navigation(c => c.Veiculos)
                .AutoInclude();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CadastroContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> CommitAsync()
        {
            return await base.SaveChangesAsync() > 0;
        }
    }
}
