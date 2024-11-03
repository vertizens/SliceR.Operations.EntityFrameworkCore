using Vertizens.SliceR;
using Vertizens.SliceR.Operations;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public class NoFilterQueryableHandler<TEntity>(
    IEntityDbContextResolver _entityDbContextResolver
    ) : IHandler<NoFilter, IQueryable<TEntity>>
    where TEntity : class
{
    public Task<IQueryable<TEntity>> Handle(NoFilter request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        return Task.FromResult(dbContext.Set<TEntity>().AsQueryable());
    }
}
