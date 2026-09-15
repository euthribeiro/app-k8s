using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wrench.auto.repair.core.Data;
using wrench.auto.repair.core.DomainObjects;
using wrench.auto.repair.core.Pagination;
using wrench.auto.repair.estoque.infra.Context;

namespace wrench.auto.repair.estoque.infra.Repositories
{
    public class Repository<TEntity>(PecaDbContext _context) : IRepository<TEntity>
        where TEntity : Entity, IAggregateRoot
    {
        protected readonly DbSet<TEntity> DbSet = _context.Set<TEntity>();

        IUnitOfWork IRepository<TEntity>.UnitOfWork => _context;

        public virtual async Task<TEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> ObterTodosAsync(CancellationToken cancellationToken)
        {
            return await DbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> Buscar(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task Adicionar(TEntity entidade, CancellationToken cancellationToken)
        {
            await DbSet.AddAsync(entidade, cancellationToken);
        }

        public virtual async Task Atualizar(TEntity entidade)
        {
            DbSet.Update(entidade);
        }

        public virtual async Task Remover(TEntity entidade)
        {
            DbSet.Remove(entidade);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<ResultadoPaginado<TEntity>> BuscaPaginadaAsync(
            RequisicaoPaginada request,
            Dictionary<string, Expression<Func<TEntity, object?>>> sortMap,
            CancellationToken cancellationToken)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            ValidarOrdenacao(request, sortMap);

            if (!string.IsNullOrWhiteSpace(request.OrdenarPor)
                && sortMap.TryGetValue(request.OrdenarPor, out var orderExpression))
            {
                query = request.Decrescente
                    ? query.OrderByDescending(orderExpression)
                    : query.OrderBy(orderExpression);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.NumeroPagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return new ResultadoPaginado<TEntity>(
                items,
                totalCount,
                request.NumeroPagina,
                request.TamanhoPagina,
                sortMap.Keys);
        }

        private static Expression<Func<TEntity, object>> BuildOrderExpression<TEntity>(string propertyPath)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");

            Expression body = parameter;
            Type currentType = typeof(TEntity);

            foreach (var member in propertyPath.Split('.'))
            {
                var property = currentType.GetProperty(member);

                if (property == null)
                    throw new InvalidOperationException($"Propriedade '{member}' não encontrada em '{currentType.Name}'");

                body = Expression.Property(body, property);
                currentType = property.PropertyType;
            }

            if (!IsScalar(currentType))
            {
                var scalarProperty = currentType
                    .GetProperties()
                    .FirstOrDefault(p => IsScalar(p.PropertyType));

                if (scalarProperty == null)
                    throw new InvalidOperationException($"Nenhuma propriedade escalar encontrada em '{currentType.Name}'");

                body = Expression.Property(body, scalarProperty);
            }

            var converted = Expression.Convert(body, typeof(object));

            return Expression.Lambda<Func<TEntity, object>>(converted, parameter);
        }

        private static bool IsScalar(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(Guid);
        }

        private static void ValidarOrdenacao<TEntity>(
            RequisicaoPaginada request,
            Dictionary<string, Expression<Func<TEntity, object?>>> sortMap)
        {
            if (string.IsNullOrWhiteSpace(request.OrdenarPor))
                return;

            if (!sortMap.ContainsKey(request.OrdenarPor))
            {
                var campos = string.Join(", ", sortMap.Keys);

                throw new ArgumentException(
                    $"A ordenação só é permitida para os campos: {campos}");
            }
        }
    }
}
