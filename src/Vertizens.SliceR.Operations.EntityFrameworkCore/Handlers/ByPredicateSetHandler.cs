namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handler that filter entities given a predicate with a set of filter values and expects a <see cref="IQueryable{TEntity}"/>
/// </summary>
/// <typeparam name="TFilter">The filter type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
public abstract class ByPredicateSetHandler<TFilter, TEntity>(IEntityDbContextResolver _entityDbContextResolver)
    where TEntity : class
{
    /// <summary>
    /// Handler that filter entities given a predicate with a set of filter values and expects a <see cref="IQueryable{TEntity}"/>
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual Task<IQueryable<TEntity>> Handle(ByPredicateSet<TFilter, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        var lambda = request.CreateFilterExpression();
        return Task.FromResult(dbContext.Set<TEntity>().Where(lambda).AsQueryable());
    }
}
