using Microsoft.EntityFrameworkCore;
using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler that filter entities given a predicate with a filter value
/// </summary>
/// <typeparam name="TFilter">The filter type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <param name="EntityDbContextResolver">The entity db context resolver.</param>
public abstract class ByPredicateHandler<TFilter, TEntity>(IEntityDbContextResolver EntityDbContextResolver)
    where TEntity : class
{
    /// <summary>
    /// Handler that filter entities given a predicate with a filter value
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async virtual Task<TEntity?> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return await dbContext.Set<TEntity>().Where(lambda).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler that filter entities given a predicate with a filter value and projects to a domain
/// </summary>
/// <typeparam name="TFilter">The filter type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDomain">Domain type to project entity to</typeparam>
/// <param name="EntityDbContextResolver">The entity db context resolver.</param>
/// <param name="_typeProjector">The type projector.</param>
public abstract class ByPredicateHandler<TFilter, TEntity, TDomain>(
    IEntityDbContextResolver EntityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector
    )
    where TEntity : class
    where TDomain : class, new()
{
    /// <summary>
    /// Handler that filter entities given a predicate with a filter value and projects to a domain
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async virtual Task<TDomain?> Handle(ByPredicate<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = EntityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return await dbContext.Set<TEntity>().Where(lambda).Select(_typeProjector.GetProjection()).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
