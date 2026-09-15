using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.autenticacao.infra.Security;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.infra.Seeds
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AutenticacaoContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var passwordHash = passwordHasher.GerarHash(password);

            var exists = await context.Usuarios.AnyAsync(u => u.Email.Endereco == email);

            if (exists) return;

            var adminPerfil = await context.Perfis
                .FirstOrDefaultAsync(p => p.Nome == "Admin");

            if (adminPerfil == null) return;

            var admin = new Usuario(new Email(email), adminPerfil.Id, true, DateTime.UtcNow);

            admin.DefinirSenha(passwordHash);

            context.Usuarios.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}
