namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler for inserting an entity
/// </summary>
/// <typeparam name="TEntity">Type of entity to insert</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
public class InsertHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<Insert<TEntity>, TEntity>
    where TEntity : class
{
    /// <summary>
    /// Handler for inserting an entity
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<TEntity> Handle(Insert<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request;
    }
}
