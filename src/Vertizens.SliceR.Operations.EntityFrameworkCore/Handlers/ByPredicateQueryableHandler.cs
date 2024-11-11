using Vertizens.TypeMapper;

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

public abstract class ByPredicateQueryableHandler<TFilter, TEntity, TDomain>(
    IEntityDbContextResolver EntityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector
    )
    where TEntity : class
    where TDomain : class, new()
{
    public virtual Task<IQueryable<TDomain>> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return Task.FromResult(dbContext.Set<TEntity>().Where(lambda).Select(_typeProjector.GetProjection()).AsQueryable());
    }
}
