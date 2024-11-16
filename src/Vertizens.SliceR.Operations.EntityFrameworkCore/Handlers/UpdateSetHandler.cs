namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler for updating a set of entities
/// </summary>
/// <typeparam name="TEntity">Type of entity to update</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
public class UpdateSetHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<UpdateSet<TEntity>, IEnumerable<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Handler for updating a set of entities
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<IEnumerable<TEntity>> Handle(UpdateSet<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().UpdateRange(request.Entities);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request.Entities;
    }
}
