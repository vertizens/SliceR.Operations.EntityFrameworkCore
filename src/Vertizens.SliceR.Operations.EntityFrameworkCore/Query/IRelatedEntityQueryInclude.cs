namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Gets a list of IQueryable includes by navigation properties for a <typeparamref name="TEntity"/> with with requirements defined
/// by related entities required when mapping from <typeparamref name="TMappedDomain"/>
/// </summary>
/// <typeparam name="TEntity">Entity that is root for includes</typeparam>
/// <typeparam name="TMappedDomain">a Domain that will be used to determine what related entities are required</typeparam>
public interface IRelatedEntityQueryInclude<TMappedDomain, TEntity>
{
    /// <summary>
    /// Returns include definitions for IQueryable.  Each definition may be multilevel depth
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetIncludes();
}
