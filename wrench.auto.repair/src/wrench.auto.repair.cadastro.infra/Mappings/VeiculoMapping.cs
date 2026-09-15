using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wrench.auto.repair.cadastro.domain.Entities;

namespace wrench.auto.repair.cadastro.infra.Mappings
{
    public class VeiculoMapping : IEntityTypeConfiguration<Veiculo>
    {
        public void Configure(EntityTypeBuilder<Veiculo> builder)
        {
            builder
                .HasIndex(v => v.PlacaDoVeiculo)
                .IsUnique();

            builder
                .HasOne(c => c.Cliente)
                .WithMany(c => c.Veiculos)
                .HasForeignKey(v => v.ClienteId)
                .IsRequired();
        }
    }
}
