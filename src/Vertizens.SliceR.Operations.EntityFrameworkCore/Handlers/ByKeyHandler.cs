using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handles a <see cref="ByKey{TKey}"/> request for an entity
/// </summary>
/// <typeparam name="TKey">The key type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
/// <param name="_keyPredicate">The entity key expression builder.</param>
public class ByKeyHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IKeyPredicate<TKey, TEntity> _keyPredicate
    )
    : ByPredicateHandler<TKey, TEntity>(_entityDbContextResolver),
    IHandler<ByKey<TKey>, TEntity?>
    where TEntity : class
{
    /// <summary>
    /// Handles a <see cref="ByKey{TKey}"/> request for an entity
    /// </summary>
    /// <param name="request">The ByKey request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<TEntity?> Handle(ByKey<TKey> request, CancellationToken cancellationToken = default)
    {
        var keyExpression = _keyPredicate.GetPredicate();
        return Handle(new ByPredicate<TKey, TEntity>(request, keyExpression), cancellationToken);
    }
}

/// <summary>
/// Handles a <see cref="ByKey{TKey}"/> request for an entity
/// </summary>
/// <typeparam name="TKey">Key type</typeparam>
/// <typeparam name="TEntity">Entity type to find</typeparam>
/// <typeparam name="TDomain">Domain type to project entity to</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
/// <param name="_typeProjector">The type projector.</param>
/// <param name="_keyPredicate">The key predicate expression builder.</param>
public class ByKeyHandler<TKey, TEntity, TDomain>(
    IEntityDbContextResolver _entityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector,
    IKeyPredicate<TKey, TEntity> _keyPredicate
    )
    : ByPredicateHandler<TKey, TEntity, TDomain>(_entityDbContextResolver, _typeProjector),
    IHandler<ByKey<TKey>, TDomain?>
    where TEntity : class
    where TDomain : class, new()
{
    /// <summary>
    /// Handles a <see cref="ByKey{TKey}"/> request for an entity
    /// </summary>
    /// <param name="request">The ByKey request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<TDomain?> Handle(ByKey<TKey> request, CancellationToken cancellationToken = default)
    {
        var keyExpression = _keyPredicate.GetPredicate();
        return Handle(new ByPredicate<TKey, TEntity>(request, keyExpression), cancellationToken);
    }
}
