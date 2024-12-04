using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Defines a predicate to use to filter an entity by its key type
/// </summary>
/// <typeparam name="TKey">Key type</typeparam>
/// <typeparam name="TEntity">Entity Type</typeparam>
public interface IKeyPredicate<TKey, TEntity>
{
    /// <summary>
    /// Get a predicate for filtering to find an Entity by its Key
    /// </summary>
    /// <returns>Entity filtered by a key expression</returns>
    Expression<Func<TEntity, TKey, bool>> GetPredicate();
}
