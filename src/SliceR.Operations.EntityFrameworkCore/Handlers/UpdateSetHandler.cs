using Vertizens.SliceR;
using Vertizens.SliceR.Operations;

namespace SliceR.Operations.EntityFrameworkCore;
public class UpdateSetHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<UpdateSet<TEntity>, IEnumerable<TEntity>>
    where TEntity : class
{
    public async Task<IEnumerable<TEntity>> Handle(UpdateSet<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().UpdateRange(request.Entities);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request.Entities;
    }
}
