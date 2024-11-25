using Microsoft.EntityFrameworkCore;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handles a <see cref="ByKeyForUpdate{TKey,TUpdateDomain}"/> request for an entity
/// </summary>
/// <typeparam name="TKey">The key type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TUpdateDomain">The domain that will map to the entity</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
/// <param name="_entityKeyExpressionBuilder">The entity key expression builder.</param>
/// <param name="_queryInclude">The resolved includes needed for an update when <typeparamref name="TUpdateDomain"/> maps to <typeparamref name="TEntity"/></param>
public class ByKeyForUpdateHandler<TKey, TUpdateDomain, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IEntityKeyExpressionBuilder<TKey, TEntity> _entityKeyExpressionBuilder,
    IRelatedEntityQueryInclude<TUpdateDomain, TEntity> _queryInclude
    )
    : IHandler<ByKeyForUpdate<TKey, TUpdateDomain>, TEntity?>
    where TEntity : class
{
    /// <summary>
    /// Handles a <see cref="ByKeyForUpdate{TKey,TUpdateDomain}"/> request for an entity
    /// </summary>
    /// <param name="request">The ByKey request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<TEntity?> Handle(ByKeyForUpdate<TKey, TUpdateDomain> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        var keyExpression = _entityKeyExpressionBuilder.Build();
        var filterExpression = (new ByPredicate<TKey, TEntity>(request.Key, keyExpression)).CreateFilterExpression();
        var query = dbContext.Set<TEntity>().Where(filterExpression);

        var includes = _queryInclude.GetIncludes();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
