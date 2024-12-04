using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Defines an implementation of an expression builder for filtering entities by a key value
/// </summary>
/// <typeparam name="TKey">Key type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IKeyPredicateExpressionBuilder<TKey, TEntity>
{
    /// <summary>
    /// Apply a part of an expression that equates an entity property to a key or key property
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <param name="propertySelector">Expression to select a property</param>
    /// <param name="valueSelector">expression to select a value</param>
    /// <returns></returns>
    IKeyPredicateExpressionBuilder<TKey, TEntity> Equal<T>(Expression<Func<TEntity, T>> propertySelector, Expression<Func<TKey, T>> valueSelector);

    /// <summary>
    /// Apply default Name match rules to match properties by name and type between key and entity
    /// </summary>
    /// <returns></returns>
    IKeyPredicateExpressionBuilder<TKey, TEntity> ApplyNameMatch();

    /// <summary>
    /// Builds an expression that filters entities to match a key value
    /// </summary>
    /// <returns></returns>
    Expression<Func<TEntity, TKey, bool>> Build();
}
