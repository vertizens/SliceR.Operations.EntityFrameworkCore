using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Defines an implementation of an expression builder for filtering entities by a set of key values
/// </summary>
/// <typeparam name="TKey">Key type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IEntityKeySetExpressionBuilder<TKey, TEntity>
{
    /// <summary>
    /// Builds an expression that filters entities to match a set of key values
    /// </summary>
    /// <returns></returns>
    Expression<Func<TEntity, ICollection<TKey>, bool>> Build();
}
