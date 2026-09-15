using System.Linq.Expressions;
using wrench.auto.repair.autenticacao.domain.Entities;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.autenticacao.domain.Data
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken);

        Task<Perfil?> ObterPerfilPorIdAsync(Guid id);

        Task<IEnumerable<Perfil>> ObterTodosPerfisAsync(CancellationToken cancellationToken);

        Task<IEnumerable<Perfil>> BuscarPerfil(Expression<Func<Perfil, bool>> predicate, CancellationToken cancellationToken);
    }
}
