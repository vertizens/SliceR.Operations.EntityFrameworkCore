namespace Vertizens.SliceR.Operations.EntityFrameworkCore;


/// <summary>
/// Handler to find entities by a set of key values
/// </summary>
/// <typeparam name="TKey">The key type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
/// <param name="_entityKeySetExpressionBuilder">The entity key set expression builder.</param>
public class ByKeySetHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IEntityKeySetExpressionBuilder<TKey, TEntity> _entityKeySetExpressionBuilder
    )
    : ByPredicateSetHandler<TKey, TEntity>(_entityDbContextResolver),
    IHandler<ByKeySet<TKey>, IQueryable<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Handler to find entities by a set of key values
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<IQueryable<TEntity>> Handle(ByKeySet<TKey> request, CancellationToken cancellationToken = default)
    {
        var keyExpression = _entityKeySetExpressionBuilder.Build();
        return Handle(new ByPredicateSet<TKey, TEntity>(request.Keys, keyExpression), cancellationToken);
    }
}
