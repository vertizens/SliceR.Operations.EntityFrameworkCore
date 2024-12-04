using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class PropertyKeyPredicate<TKey, TEntity>(
    string _keyPropertyName)
    : IKeyPredicate<TKey, TEntity>
{
    private Expression<Func<TEntity, TKey, bool>>? _expressionCache;
    public Expression<Func<TEntity, TKey, bool>> GetPredicate()
    {
        _expressionCache ??= Build(_keyPropertyName);
        return _expressionCache;
    }

    private static Expression<Func<TEntity, TKey, bool>> Build(string keyPropertyName)
    {
        var keyType = typeof(TKey);
        var parameterEntity = Expression.Parameter(typeof(TEntity), "x");
        var parameterKey = Expression.Parameter(keyType, "key");

        var entityProperty = Expression.Property(parameterEntity, typeof(TEntity).GetProperty(keyPropertyName)!);
        var body = Expression.Equal(entityProperty, parameterKey);
        var expression = Expression.Lambda<Func<TEntity, TKey, bool>>(body, parameterEntity, parameterKey);

        return expression;
    }


}
