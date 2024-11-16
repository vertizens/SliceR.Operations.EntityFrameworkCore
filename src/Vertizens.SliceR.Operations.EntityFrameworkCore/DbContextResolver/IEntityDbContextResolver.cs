using Microsoft.EntityFrameworkCore;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Defines an implementation that resolves a <see cref="DbContext"/> or <see cref="IDbContextFactory{TContext}"/> for an entity
/// </summary>
public interface IEntityDbContextResolver
{
    /// <summary>
    /// Instance of DbContext for entity <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Entity Type</typeparam>
    /// <returns></returns>
    DbContext Resolve<T>();

    /// <summary>
    /// Instance of <see cref="IDbContextFactory{TContext}"/> for entity <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Entity Type</typeparam>
    /// <returns></returns>
    IDbContextFactory<DbContext> ResolveFactory<T>();
}
