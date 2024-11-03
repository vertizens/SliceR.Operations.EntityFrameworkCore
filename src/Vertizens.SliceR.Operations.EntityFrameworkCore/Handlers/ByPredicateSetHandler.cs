using Vertizens.SliceR.Operations;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public abstract class ByPredicateSetHandler<TFilter, TEntity>(IEntityDbContextResolver _entityDbContextResolver)
    where TEntity : class
{
    public virtual Task<IQueryable<TEntity>> Handle(ByPredicateSet<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return Task.FromResult(dbContext.Set<TEntity>().Where(lambda).AsQueryable());
    }
}
