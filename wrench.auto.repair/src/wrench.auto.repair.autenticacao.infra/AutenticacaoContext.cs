using Microsoft.EntityFrameworkCore;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Data;

namespace wrench.auto.repair.autenticacao.infra
{
    public class AutenticacaoContext(
        DbContextOptions<AutenticacaoContext> options
    ) : DbContext(options), IUnitOfWork
    {
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Perfil> Perfis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutenticacaoContext).Assembly);

            modelBuilder.Entity<Usuario>()
              .Navigation(c => c.Perfil)
              .AutoInclude();

            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> CommitAsync()
        {
            return await base.SaveChangesAsync() > 0;
        }
    }
}
