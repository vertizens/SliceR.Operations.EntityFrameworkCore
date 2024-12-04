using System.Linq.Expressions;
using System.Reflection;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class DefaultKeyPredicateExpressionBuilder<TKey, TEntity> : IKeyPredicateExpressionBuilder<TKey, TEntity>
{
    private readonly ParameterExpression parameterKey = Expression.Parameter(typeof(TKey), "key");
    private readonly ParameterExpression parameterEntity = Expression.Parameter(typeof(TEntity), "entity");
    private readonly IList<Expression> _expressions = [];

    public IKeyPredicateExpressionBuilder<TKey, TEntity> Equal<T>(Expression<Func<TEntity, T>> propertySelector, Expression<Func<TKey, T>> valueSelector)
    {
        var propertyExpression = ReplaceParameterExpressionVisitor.ReplaceParameter(propertySelector.Body, propertySelector.Parameters[0], parameterEntity);
        var valueExpression = ReplaceParameterExpressionVisitor.ReplaceParameter(valueSelector.Body, valueSelector.Parameters[0], parameterKey);
        var propertyAssignment = Expression.Equal(propertyExpression, valueExpression);
        _expressions.Add(propertyAssignment);

        return this;
    }

    public IKeyPredicateExpressionBuilder<TKey, TEntity> ApplyNameMatch()
    {
        var keyGetProperties = typeof(TKey).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetMethod?.IsPublic == true);
        var entityGetProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetMethod?.IsPublic == true).ToDictionary(x => x.Name);

        foreach (var keyGetProperty in keyGetProperties)
        {
            if (entityGetProperties.TryGetValue(keyGetProperty.Name, out var entityGetProperty))
            {
                var expression = BuildPropertyEqual(parameterKey, keyGetProperty, entityGetProperty);
                if (expression != null)
                {
                    _expressions.Add(expression);
                }
            }
        }

        return this;
    }

    public Expression<Func<TEntity, TKey, bool>> Build()
    {
        Expression bodyExpression = Expression.Constant(false);
        if (_expressions.Count > 0)
        {
            bodyExpression = _expressions[0];

            foreach (var expression in _expressions.Skip(1))
            {
                bodyExpression = Expression.AndAlso(bodyExpression, expression);
            }
        }

        return Expression.Lambda<Func<TEntity, TKey, bool>>(bodyExpression, parameterKey, parameterEntity);
    }

    private static BinaryExpression? BuildPropertyEqual(ParameterExpression parameterKey, PropertyInfo keyGetProperty, PropertyInfo entityGetProperty)
    {
        BinaryExpression? expression = null;
        if (entityGetProperty.PropertyType.IsAssignableFrom(keyGetProperty.PropertyType))
        {
            expression = BuildAssignablePropertyEqual(parameterKey, keyGetProperty, entityGetProperty);
        }

        return expression;
    }

    private static BinaryExpression BuildAssignablePropertyEqual(ParameterExpression parameterKey, PropertyInfo keyGetProperty, PropertyInfo entityGetProperty)
    {
        Expression sourceProperty = Expression.Property(parameterKey, keyGetProperty);
        Expression targetProperty = Expression.Property(parameterKey, entityGetProperty);
        if (keyGetProperty.PropertyType != entityGetProperty.PropertyType)
        {
            sourceProperty = Expression.ConvertChecked(sourceProperty, entityGetProperty.PropertyType);
        }
        var propertyBinding = Expression.Equal(targetProperty, sourceProperty);

        return propertyBinding;
    }
}
