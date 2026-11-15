using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.infra.Saga;

namespace wrench.auto.repair.ordem.servico.infra.Mapping
{
    public class SagaOrdemServicoMapping : IEntityTypeConfiguration<SagaOrdemServico>
    {
        public void Configure(EntityTypeBuilder<SagaOrdemServico> builder)
        {
            builder.ToTable("SagaOrdemServico");

            builder.Property(x => x.CorrelationId).IsRequired().HasMaxLength(100);
            builder.Property(x => x.MotivoDesfecho).HasMaxLength(500);
            builder.Property(x => x.ValorAprovado).HasColumnType("decimal(18,2)");

            builder.HasIndex(x => x.OrdemServicoId).IsUnique();

            // A varredura de timeouts procura por prazo vencido dentro dos estados em espera.
            builder.HasIndex(x => new { x.Estado, x.PrazoEtapa });

            builder.OwnsMany(x => x.Transicoes, t =>
            {
                t.ToTable("SagaTransicao");

                t.WithOwner().HasForeignKey("SagaOrdemServicoId");

                t.HasKey(x => x.Id);

                t.Property(x => x.Gatilho).IsRequired().HasMaxLength(100);
                t.Property(x => x.Motivo).HasMaxLength(500);
                t.Property(x => x.OcorridaEm).IsRequired();
            });

            builder.Navigation(x => x.Transicoes)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    public class OutboxMensagemMapping : IEntityTypeConfiguration<OutboxMensagem>
    {
        public void Configure(EntityTypeBuilder<OutboxMensagem> builder)
        {
            builder.ToTable("SagaOutbox");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Tipo).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.CorrelationId).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Traceparent).HasMaxLength(100);
            builder.Property(x => x.UltimoErro).HasMaxLength(1000);

            // O dispatcher varre as pendentes em ordem de criacao.
            builder.HasIndex(x => new { x.PublicadaEm, x.CriadaEm });
        }
    }

    public class InboxMensagemMapping : IEntityTypeConfiguration<InboxMensagem>
    {
        public void Configure(EntityTypeBuilder<InboxMensagem> builder)
        {
            builder.ToTable("SagaInbox");

            builder.HasKey(x => x.MessageId);

            builder.Property(x => x.MessageId).HasMaxLength(100);
            builder.Property(x => x.Tipo).IsRequired().HasMaxLength(100);
            builder.Property(x => x.RecebidaEm).IsRequired();
        }
    }
}
