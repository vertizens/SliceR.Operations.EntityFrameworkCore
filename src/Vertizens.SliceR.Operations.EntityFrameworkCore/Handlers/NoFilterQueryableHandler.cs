using Microsoft.EntityFrameworkCore;
using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Handles a request with no filter on entities and returns <see cref="IQueryable{TEntity}"/>
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
public class NoFilterQueryableHandler<TEntity>(
    IEntityDbContextResolver _entityDbContextResolver
    ) : IHandler<NoFilter, IQueryable<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Handles a request with no filter on entities and returns <see cref="IQueryable{TEntity}"/>
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<IQueryable<TEntity>> Handle(NoFilter request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        return Task.FromResult(dbContext.Set<TEntity>().AsQueryable().AsNoTracking());
    }
}

/// <summary>
/// Handles a request with no filter on entities and returns <see cref="IQueryable{TDomain}"/>
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TDomain">Domain type</typeparam>
/// <param name="_entityDbContextResolver">The entity db context resolver.</param>
/// <param name="_typeProjector">The type projector.</param>
public class NoFilterQueryableHandler<TEntity, TDomain>(
    IEntityDbContextResolver _entityDbContextResolver,
    ITypeProjector<TEntity, TDomain> _typeProjector
    ) : IHandler<NoFilter, IQueryable<TDomain>>
    where TEntity : class
    where TDomain : class, new()
{
    /// <summary>
    /// Handles a request with no filter on entities and returns <see cref="IQueryable{TDomain}"/>
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<IQueryable<TDomain>> Handle(NoFilter request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        return Task.FromResult(dbContext.Set<TEntity>().Select(_typeProjector.GetProjection()).AsQueryable());
    }
}
