using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wrench.auto.repair.autenticacao.domain.Entities;

namespace wrench.auto.repair.autenticacao.infra.Mappings
{
    public class PerfilMapping : IEntityTypeConfiguration<Perfil>
    {
        public void Configure(EntityTypeBuilder<Perfil> builder)
        {
            builder.HasKey(p => p.Id);

            var dataFixa = new DateTime(2026, 03, 06, 19, 0, 0, DateTimeKind.Utc);

            var perfilAdminstrativo = new Perfil("Admin", "Perfil Adminstrativo", true, dataFixa)
            {
                Id = Guid.Parse("63A459DE-75BA-4FAA-BD0E-9FCA7C9063AB")
            };

            var perfilFuncionario = new Perfil("Funcionario", "Perfil Funcionário", true, dataFixa)
            {
                Id = Guid.Parse("5665D39C-4907-40A9-B648-E9CBB041AFED")
            };

            var perfilCliente = new Perfil("Cliente", "Perfil Cliente", true, dataFixa)
            {
                Id = Guid.Parse("70B89DCE-8EA8-461C-B7C2-47A40AB906BA")
            };

            builder.HasData([
                perfilAdminstrativo,
                perfilFuncionario,
                perfilCliente
            ]);
        }
    }
}
