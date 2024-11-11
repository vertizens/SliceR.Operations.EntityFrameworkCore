using Vertizens.TypeMapper;

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

public class ByKeyHandler<TKey, TEntity, TDomain>(
    IEntityDbContextResolver _entityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector,
    IEntityKeyExpressionBuilder<TKey, TEntity> _entityKeyExpressionBuilder
    )
    : ByPredicateHandler<TKey, TEntity, TDomain>(_entityDbContextResolver, _typeProjector),
    IHandler<ByKey<TKey>, TDomain?>
    where TEntity : class
    where TDomain : class, new()
{
    public Task<TDomain?> Handle(ByKey<TKey> request, CancellationToken cancellationToken = default)
    {
        var keyExpression = _entityKeyExpressionBuilder.Build();
        return Handle(new ByPredicate<TKey, TEntity>(request, keyExpression), cancellationToken);
    }
}
