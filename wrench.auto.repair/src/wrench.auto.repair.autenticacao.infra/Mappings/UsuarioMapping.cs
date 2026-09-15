using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wrench.auto.repair.autenticacao.domain.Entities;

namespace wrench.auto.repair.autenticacao.infra.Mappings
{
    public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(u => u.Id);

            builder.OwnsOne(x => x.Email, email =>
            {
                email.WithOwner();

                email.Ignore(e => e.Dominio);

                email.Property(e => e.Endereco)
                    .HasColumnName("Email")
                    .IsRequired();

                email.HasIndex(e => e.Endereco).IsUnique();
            });

            builder
                .HasOne<Perfil>(u => u.Perfil)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(u => u.PerfilId)
                .IsRequired();
        }
    }
}
