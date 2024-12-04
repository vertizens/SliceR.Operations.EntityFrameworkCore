using Microsoft.EntityFrameworkCore;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler that deletes entities given a key value, return true if entity is deleted
/// </summary>
/// <typeparam name="TKey">key type</typeparam>
/// <typeparam name="TEntity">entity type</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
/// <param name="_keyPredicate">The key predicate expression builder.</param>
public class DeleteHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IKeyPredicate<TKey, TEntity> _keyPredicate
    ) : IHandler<Delete<TKey, TEntity>, bool>
    where TEntity : class
{
    /// <summary>
    /// Handler that deletes entities given a key value, return true if entity is deleted
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<bool> Handle(Delete<TKey, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        var predicate = _keyPredicate.GetPredicate();
        var expression = new ByPredicate<TKey, TEntity>(request.Key, predicate).CreateFilterExpression();
        var recordsAffected = await dbContext.Set<TEntity>().Where(expression).ExecuteDeleteAsync(cancellationToken);

        return recordsAffected > 0;
    }
}
