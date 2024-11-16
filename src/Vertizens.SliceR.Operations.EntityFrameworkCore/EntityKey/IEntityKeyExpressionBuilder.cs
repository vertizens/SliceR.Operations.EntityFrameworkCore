using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Defines an implementation of an expression builder for filtering entities by a key value
/// </summary>
/// <typeparam name="TKey">Key type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IEntityKeyExpressionBuilder<TKey, TEntity>
{
    /// <summary>
    /// Builds an expression that filters entities to match a key value
    /// </summary>
    /// <returns></returns>
    Expression<Func<TEntity, TKey, bool>> Build();
}
