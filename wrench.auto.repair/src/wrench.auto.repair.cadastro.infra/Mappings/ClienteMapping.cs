using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wrench.auto.repair.cadastro.domain.Entities;

namespace wrench.auto.repair.cadastro.infra.Mappings
{
    public class ClienteMapping : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.OwnsOne(x => x.Email, email =>
            {
                email.WithOwner();

                email.Ignore(e => e.Dominio);

                email.Property(e => e.Endereco)
                    .HasColumnName("Email")
                    .IsRequired();

                email.HasIndex(e => e.Endereco).IsUnique();
            });

            builder.OwnsOne(c => c.Documento, documento =>
            {
                documento.WithOwner();

                documento.Ignore(d => d.TipoDocumento);

                documento.Property(d => d.Numeracao)
                    .HasColumnName("Documento")
                    .IsRequired();

                documento.HasIndex(d => d.Numeracao).IsUnique();
            });

            builder.OwnsOne(c => c.Nome, nome =>
            {
                nome.WithOwner();

                nome.Property(d => d.Nome)
                    .HasColumnName("Nome")
                    .IsRequired();
            });

            builder.OwnsOne(c => c.Telefone, telefone =>
            {
                telefone.ToTable("ClienteTelefones");
            });

            builder.OwnsOne(c => c.Endereco, e =>
            {
                e.ToTable("ClienteEnderecos");
            });
        }
    }
}
