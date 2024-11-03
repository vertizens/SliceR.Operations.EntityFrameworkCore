namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public abstract class ByPredicateQueryableHandler<TFilter, TEntity>(IEntityDbContextResolver EntityDbContextResolver)
    where TEntity : class
{
    public virtual Task<IQueryable<TEntity>> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return Task.FromResult(dbContext.Set<TEntity>().Where(lambda).AsQueryable());
    }
}
