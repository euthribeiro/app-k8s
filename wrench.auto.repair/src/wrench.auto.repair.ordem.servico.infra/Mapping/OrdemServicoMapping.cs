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

            builder.OwnsMany(x => x.Historico, h =>
            {
                h.ToTable("OrdemServicoHistorico");

                h.WithOwner().HasForeignKey("OrdemServicoId");

                h.Property(x => x.StatusNovo).IsRequired();
                h.Property(x => x.Motivo).HasMaxLength(500);
                h.Property(x => x.CorrelationId).HasMaxLength(100);
                h.Property(x => x.OcorridoEm).IsRequired();
            });

            builder.Navigation(x => x.Historico)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.ClienteId);

            builder.HasIndex(x => new { x.Status, x.DataCriacao });
        }
    }
}
