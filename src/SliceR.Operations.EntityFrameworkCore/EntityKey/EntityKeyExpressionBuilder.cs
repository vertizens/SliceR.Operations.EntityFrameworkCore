using System.Linq.Expressions;

namespace SliceR.Operations.EntityFrameworkCore;
internal class EntityKeyExpressionBuilder<TKey, TEntity>(
    ICollection<string> _keyPropertyNames)
    : IEntityKeyExpressionBuilder<TKey, TEntity>
{
    private Expression<Func<TEntity, TKey, bool>>? _expressionCache;
    public Expression<Func<TEntity, TKey, bool>> Build()
    {
        _expressionCache ??= Build(_keyPropertyNames);
        return _expressionCache;
    }

    private static Expression<Func<TEntity, TKey, bool>> Build(ICollection<string> keyPropertyNames)
    {
        var keyType = typeof(TKey);
        var parameterEntity = Expression.Parameter(typeof(TEntity), "x");
        var parameterKey = Expression.Parameter(keyType, "key");

        Expression? body = null;
        if (keyPropertyNames.Count > 1)
        {
            var propertyIndex = 1;
            foreach (var keyPropertyName in keyPropertyNames)
            {
                var entityProperty = Expression.Property(parameterEntity, typeof(TEntity).GetProperty(keyPropertyName)!);
                var keyProperty = Expression.Property(parameterKey, keyType.GetProperty(keyPropertyName)!) ??
                    Expression.Property(parameterKey, keyType.GetProperty("Item" + propertyIndex)!);
                var equalityExpression = Expression.Equal(entityProperty, keyProperty);
                body = body == null ? equalityExpression : Expression.And(body, equalityExpression);
                propertyIndex++;
            }
        }
        else
        {
            var entityProperty = Expression.Property(parameterEntity, typeof(TEntity).GetProperty(keyPropertyNames.First())!);
            body = Expression.Equal(entityProperty, parameterKey);
        }

        var expression = Expression.Lambda<Func<TEntity, TKey, bool>>(body!, parameterEntity, parameterKey);

        return expression;
    }
}
