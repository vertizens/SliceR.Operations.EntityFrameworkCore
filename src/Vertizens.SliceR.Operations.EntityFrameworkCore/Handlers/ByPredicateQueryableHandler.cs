using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
/// <summary>
/// Handler that filter entities given a predicate with a filter value and expects a <see cref="IQueryable{TEntity}"/>
/// </summary>
/// <typeparam name="TFilter">The filter type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <param name="EntityDbContextResolver">The entity db context resolver.</param>
public abstract class ByPredicateQueryableHandler<TFilter, TEntity>(IEntityDbContextResolver EntityDbContextResolver)
    where TEntity : class
{
    /// <summary>
    /// Handler that filter entities given a predicate with a filter value and expects a <see cref="IQueryable{TEntity}"/>
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual Task<IQueryable<TEntity>> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return Task.FromResult(dbContext.Set<TEntity>().Where(lambda).AsQueryable());
    }
}

/// <summary>
/// Handler that filter entities given a predicate with a filter value and expects a <see cref="IQueryable{TDomain}"/>
/// </summary>
/// <typeparam name="TFilter">The filter type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDomain">The domain to project from the entity</typeparam>
/// <param name="EntityDbContextResolver">The entity db context resolver.</param>
/// <param name="_typeProjector">The type projector.</param>
public abstract class ByPredicateQueryableHandler<TFilter, TEntity, TDomain>(
    IEntityDbContextResolver EntityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector
    )
    where TEntity : class
    where TDomain : class, new()
{
    /// <summary>
    /// Handler that filter entities given a predicate with a filter value and expects a <see cref="IQueryable{TDomain}"/>
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual Task<IQueryable<TDomain>> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return Task.FromResult(dbContext.Set<TEntity>().Where(lambda).Select(_typeProjector.GetProjection()).AsQueryable());
    }
}
