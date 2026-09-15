using System.Linq.Expressions;

namespace wrench.auto.repair.core.Data
{
    public interface ISortMap<TEntity>
    {
        Dictionary<string, Expression<Func<TEntity, object?>>> Map { get; }
    }
}
