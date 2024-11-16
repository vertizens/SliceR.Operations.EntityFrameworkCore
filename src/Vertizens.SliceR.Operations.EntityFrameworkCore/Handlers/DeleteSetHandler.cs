using Microsoft.EntityFrameworkCore;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler that deletes entities given a set of key values, return the count of records entities
/// </summary>
/// <typeparam name="TKey">key type</typeparam>
/// <typeparam name="TEntity">entity type</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
/// <param name="_entityKeySetExpressionBuilder">The entity key set expression builder.</param>
public class DeleteSetHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IEntityKeySetExpressionBuilder<TKey, TEntity> _entityKeySetExpressionBuilder
    ) : IHandler<DeleteSet<TKey, TEntity>, int>
    where TEntity : class
{
    /// <summary>
    /// Handler that deletes entities given a set of key values, return the count of records entities
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<int> Handle(DeleteSet<TKey, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        var predicate = _entityKeySetExpressionBuilder.Build();
        var expression = new ByPredicate<ICollection<TKey>, TEntity>(request.Keys, predicate).CreateFilterExpression();
        var recordsAffected = await dbContext.Set<TEntity>().Where(expression).ExecuteDeleteAsync(cancellationToken);

        return recordsAffected;
    }
}
