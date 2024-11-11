using Vertizens.TypeMapper;

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

public class NoFilterQueryableHandler<TEntity, TDomain>(
    IEntityDbContextResolver _entityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector
    ) : IHandler<NoFilter, IQueryable<TDomain>>
    where TEntity : class
    where TDomain : class, new()
{
    public Task<IQueryable<TDomain>> Handle(NoFilter request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        return Task.FromResult(dbContext.Set<TEntity>().Select(_typeProjector.GetProjection()).AsQueryable());
    }
}
