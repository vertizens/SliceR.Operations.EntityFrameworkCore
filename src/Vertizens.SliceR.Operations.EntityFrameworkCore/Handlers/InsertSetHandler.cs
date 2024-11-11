namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

public class InsertSetHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<InsertSet<TEntity>, IEnumerable<TEntity>>
    where TEntity : class
{
    public async Task<IEnumerable<TEntity>> Handle(InsertSet<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().AddRange(request.Domains);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request.Domains;
    }
}
