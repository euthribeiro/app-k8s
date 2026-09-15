using System.Linq.Expressions;
using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Pagination;

namespace wrench.auto.repair.core.Data
{
    public interface IRepository<TEntity> : IDisposable
        where TEntity : Entity, IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
        Task Adicionar(TEntity entidade, CancellationToken cancellationToken);
        Task Atualizar(TEntity entidade);
        Task Remover(TEntity entidade);
        Task<TEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<TEntity>> ObterTodosAsync(CancellationToken cancellationToken);
        Task<IEnumerable<TEntity>> Buscar(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
        Task<ResultadoPaginado<TEntity>> BuscaPaginadaAsync(RequisicaoPaginada request, Dictionary<string, Expression<Func<TEntity, object?>>> sortMap, CancellationToken cancellationToken);
    }
}
