using Microsoft.EntityFrameworkCore;
using Vertizens.SliceR.Operations;

namespace SliceR.Operations.EntityFrameworkCore;
public abstract class ByPredicateHandler<TFilter, TEntity>(IEntityDbContextResolver EntityDbContextResolver)
    where TEntity : class
{
    public async virtual Task<TEntity?> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return await dbContext.Set<TEntity>().Where(lambda).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
