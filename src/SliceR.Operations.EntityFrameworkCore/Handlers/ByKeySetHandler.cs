using Vertizens.SliceR;
using Vertizens.SliceR.Operations;

namespace SliceR.Operations.EntityFrameworkCore;
public class ByKeySetHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IEntityKeySetExpressionBuilder<TKey, TEntity> _entityKeySetExpressionBuilder
    )
    : ByPredicateSetHandler<TKey, TEntity>(_entityDbContextResolver),
    IHandler<ByKeySet<TKey>, IQueryable<TEntity>>
    where TEntity : class
{
    public Task<IQueryable<TEntity>> Handle(ByKeySet<TKey> request, CancellationToken cancellationToken = default)
    {
        var keyExpression = _entityKeySetExpressionBuilder.Build();
        return Handle(new ByPredicateSet<TKey, TEntity>(request.Keys, keyExpression), cancellationToken);
    }
}
