using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wrench.auto.repair.ordem.servico.domain.Entities;

namespace wrench.auto.repair.ordem.servico.infra.Mapping
{
    public class OrdemServicoMapping : IEntityTypeConfiguration<OrdemServico>
    {
        public void Configure(EntityTypeBuilder<OrdemServico> builder)
        {
            builder.OwnsMany(x => x.Pecas, p =>
            {
                p.ToTable("OrdemServicoItem");

                p.WithOwner().HasForeignKey("OrdemServicoId");

                p.Property(x => x.PecaId).IsRequired();
                p.Property(x => x.Nome).IsRequired();
                p.Property(x => x.Quantidade).IsRequired();
                p.Property(x => x.ValorUnitario).HasColumnType("decimal(18,2)");
            });

            builder.Navigation(x => x.Pecas)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.ClienteId);

            builder.HasIndex(x => new { x.Status, x.DataCriacao });
        }
    }
}
