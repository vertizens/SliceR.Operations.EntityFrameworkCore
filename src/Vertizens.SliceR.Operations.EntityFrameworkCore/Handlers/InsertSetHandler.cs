namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler for inserting a set of entities
/// </summary>
/// <typeparam name="TEntity">Type of entity to insert</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
public class InsertSetHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<InsertSet<TEntity>, IEnumerable<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Handler for inserting a set of entities
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<IEnumerable<TEntity>> Handle(InsertSet<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().AddRange(request.Domains);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request.Domains;
    }
}
