using Vertizens.SliceR;
using Vertizens.SliceR.Operations;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public class ByKeyHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IEntityKeyExpressionBuilder<TKey, TEntity> _entityKeyExpressionBuilder
    )
    : ByPredicateHandler<TKey, TEntity>(_entityDbContextResolver),
    IHandler<ByKey<TKey>, TEntity?>
    where TEntity : class
{
    public Task<TEntity?> Handle(ByKey<TKey> request, CancellationToken cancellationToken = default)
    {
        var keyExpression = _entityKeyExpressionBuilder.Build();
        return Handle(new ByPredicate<TKey, TEntity>(request, keyExpression), cancellationToken);
    }
}
