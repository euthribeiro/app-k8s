using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wrench.auto.repair.estoque.domain.Entities;

namespace wrench.auto.repair.estoque.infra.Mapping
{
    public class PecaMapping : IEntityTypeConfiguration<Peca>
    {
        public void Configure(EntityTypeBuilder<Peca> builder)
        {
            builder
               .HasIndex(p => p.Nome)
               .IsUnique();
        }
    }
}
