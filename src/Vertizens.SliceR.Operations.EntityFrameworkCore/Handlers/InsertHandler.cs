using Vertizens.SliceR;
using Vertizens.SliceR.Operations;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public class InsertHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<Insert<TEntity>, TEntity>
    where TEntity : class
{
    public async Task<TEntity> Handle(Insert<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request;
    }
}
