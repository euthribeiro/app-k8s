using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wrench.auto.repair.autenticacao.domain.Data;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.infra.Repositories
{
    public sealed class UsuarioRepository(AutenticacaoContext _context) :
        Repository<Usuario>(_context), IUsuarioRepository
    {
        public async Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken)
        {
            return await DbSet.Include(u => u.Perfil).FirstOrDefaultAsync(u => u.Email.Endereco == email.Endereco, cancellationToken);
        }

        public override async Task<IEnumerable<Usuario>> ObterTodosAsync(CancellationToken cancellationToken)
        {
            return await DbSet.Include(u => u.Perfil).ToListAsync(cancellationToken);
        }

        public override async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await DbSet.Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<Perfil?> ObterPerfilPorIdAsync(Guid id)
        {
            return await _context.Perfis.FindAsync(id);
        }

        public async Task<IEnumerable<Perfil>> ObterTodosPerfisAsync(CancellationToken cancellationToken)
        {
            return await _context.Perfis.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Perfil>> BuscarPerfil(Expression<Func<Perfil, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _context.Perfis.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
        }
    }
}
