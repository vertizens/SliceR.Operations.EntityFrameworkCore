using Vertizens.SliceR;
using Vertizens.SliceR.Operations;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public class UpdateHandler<TEntity>(IEntityDbContextResolver _entityDbContextResolver) : IHandler<Update<TEntity>, TEntity>
    where TEntity : class
{
    public async Task<TEntity> Handle(Update<TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        dbContext.Set<TEntity>().Update(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request;
    }
}
