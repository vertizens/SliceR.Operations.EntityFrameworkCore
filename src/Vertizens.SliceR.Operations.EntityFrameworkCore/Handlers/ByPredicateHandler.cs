using Microsoft.EntityFrameworkCore;
using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
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

public abstract class ByPredicateHandler<TFilter, TEntity, TDomain>(
    IEntityDbContextResolver EntityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector
    )
    where TEntity : class
    where TDomain : class, new()
{
    public async virtual Task<TDomain?> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return await dbContext.Set<TEntity>().Where(lambda).Select(_typeProjector.GetProjection()).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
