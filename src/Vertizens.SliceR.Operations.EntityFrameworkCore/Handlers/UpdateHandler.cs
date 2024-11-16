namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler for updating an entity
/// </summary>
/// <typeparam name="TEntity">Type of entity to update</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
public class UpdateHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<Update<TEntity>, TEntity>
    where TEntity : class
{
    /// <summary>
    /// Handler for updating an entity
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<TEntity> Handle(Update<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().Update(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request;
    }
}
