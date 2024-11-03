using System.Linq.Expressions;

namespace SliceR.Operations.EntityFrameworkCore;
internal class EntityKeySetExpressionBuilder<TKey, TEntity>(
    string _keyPropertyName)
    : IEntityKeySetExpressionBuilder<TKey, TEntity>
{
    public Expression<Func<TEntity, ICollection<TKey>, bool>> Build()
    {
        var parameterEntity = Expression.Parameter(typeof(TEntity), "x");
        var parameterKeys = Expression.Parameter(typeof(ICollection<TKey>), "keys");
        var entityProperty = Expression.Property(parameterEntity, typeof(TEntity).GetProperty(_keyPropertyName)!);
        var containsMethod = typeof(ICollection<TKey>).GetMethods().First(x => x.Name == nameof(ICollection<TKey>.Contains) && x.GetParameters().Length == 2);
        containsMethod = containsMethod.MakeGenericMethod(typeof(TKey));
        var body = Expression.Call(parameterKeys, containsMethod!, entityProperty);
        var expression = Expression.Lambda<Func<TEntity, ICollection<TKey>, bool>>(body, parameterEntity, parameterKeys);

        return expression;
    }
}
