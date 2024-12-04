using System.Linq.Expressions;
using System.Reflection;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// An entity key reader that uses a key predicate to reverse how a key should be read from an entity
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <param name="_keyPredicate"></param>
internal class EntityKeyReader<TKey, TEntity>(
    IKeyPredicate<TKey, TEntity> _keyPredicate
    ) : IEntityKeyReader<TKey, TEntity>
{
    private Func<TEntity, TKey>? _readerCache;

    public TKey ReadKey(TEntity entity)
    {
        _readerCache ??= BuildReader(_keyPredicate);
        return _readerCache(entity);
    }

    private static Func<TEntity, TKey> BuildReader(IKeyPredicate<TKey, TEntity> _keyPredicate)
    {
        var keyPredicate = _keyPredicate.GetPredicate();

        var newParameterEntity = Expression.Parameter(typeof(TEntity), "entity");
        Expression<Func<TEntity, TKey>>? expression = null;

        ParameterExpression predicateParameterEntity = keyPredicate.Parameters[0];
        ParameterExpression predicateParameterKey = keyPredicate.Parameters[1];

        IList<MemberBinding> bindings = [];
        if (keyPredicate.Body.NodeType == ExpressionType.Equal)
        {
            var equalExpression = (BinaryExpression)keyPredicate.Body;

            if (equalExpression.Left is MemberExpression leftMemberExpression
                && equalExpression.Right == predicateParameterKey)
            {
                var primitiveExpression = ReplaceParameterExpressionVisitor.ReplaceParameter(leftMemberExpression, predicateParameterEntity, newParameterEntity);
                expression = Expression.Lambda<Func<TEntity, TKey>>(primitiveExpression, newParameterEntity);
            }
            else if (equalExpression.Right is MemberExpression rightMemberExpression
                && equalExpression.Left == predicateParameterKey)
            {
                var primitiveExpression = ReplaceParameterExpressionVisitor.ReplaceParameter(rightMemberExpression, predicateParameterKey, newParameterEntity);
                expression = Expression.Lambda<Func<TEntity, TKey>>(primitiveExpression, newParameterEntity);
            }
            else if (equalExpression.Left is MemberExpression memberExpression1
                && equalExpression.Right is MemberExpression memberExpression2)
            {
                AddBindingFromEqual(newParameterEntity, predicateParameterEntity, predicateParameterKey, memberExpression1, memberExpression2, bindings);
            }
        }
        else if (keyPredicate.Body.NodeType == ExpressionType.AndAlso)
        {
            var andAlsoExpression = (BinaryExpression)keyPredicate.Body;
            AddMemberBindings(andAlsoExpression.Left, newParameterEntity, predicateParameterEntity, predicateParameterKey, bindings);
            AddMemberBindings(andAlsoExpression.Right, newParameterEntity, predicateParameterEntity, predicateParameterKey, bindings);
        }

        if (expression == null && bindings.Count > 0)
        {
            var newBody = Expression.MemberInit(Expression.New(typeof(TKey)), bindings);
            expression = Expression.Lambda<Func<TEntity, TKey>>(newBody, newParameterEntity);
        }

        if (expression == null)
        {
            throw new ArgumentException("Invalid key predicate for key reader");
        }

        return expression.Compile();
    }

    private static void AddBindingFromEqual(
        ParameterExpression newParameterEntity,
        ParameterExpression predicateParameterEntity,
        ParameterExpression predicateParameterKey,
        MemberExpression memberExpression1,
        MemberExpression memberExpression2,
        IList<MemberBinding> bindings)
    {
        PropertyInfo? entityProperty = null;
        if (memberExpression1.Member.DeclaringType == predicateParameterEntity.Type)
        {
            entityProperty = (PropertyInfo)memberExpression1.Member;
        }
        else if (memberExpression2.Member.DeclaringType == predicateParameterEntity.Type)
        {
            entityProperty = (PropertyInfo)memberExpression2.Member;
        }

        PropertyInfo? keyProperty = null;
        if (memberExpression1.Member.DeclaringType == predicateParameterKey.Type)
        {
            keyProperty = (PropertyInfo)memberExpression1.Member;
        }
        else if (memberExpression2.Member.DeclaringType == predicateParameterKey.Type)
        {
            keyProperty = (PropertyInfo)memberExpression2.Member;
        }

        if (entityProperty != null && keyProperty != null)
        {
            var entityPropertyExpression = Expression.Property(newParameterEntity, entityProperty);
            Expression bindingExpression = entityProperty != keyProperty.PropertyType ? Expression.ConvertChecked(entityPropertyExpression, keyProperty.PropertyType) : entityPropertyExpression;
            var keyPropertyBinding = Expression.Bind(keyProperty, bindingExpression);
            bindings.Add(keyPropertyBinding);
        }
        else
        {
            throw new ArgumentException("Invalid key predicate equality expression");
        }
    }

    private static void AddMemberBindings(Expression expression,
        ParameterExpression newParameterEntity,
        ParameterExpression predicateParameterEntity,
        ParameterExpression predicateParameterKey,
        IList<MemberBinding> bindings)
    {
        if (expression.NodeType == ExpressionType.Equal)
        {
            var equalExpression = (BinaryExpression)expression;

            if (equalExpression.Left is MemberExpression memberExpression1
                && equalExpression.Right is MemberExpression memberExpression2)
            {
                AddBindingFromEqual(newParameterEntity, predicateParameterEntity, predicateParameterKey, memberExpression1, memberExpression2, bindings);
            }
        }
        else if (expression.NodeType == ExpressionType.AndAlso)
        {
            var andAlsoExpression = (BinaryExpression)expression;
            AddMemberBindings(andAlsoExpression.Left, newParameterEntity, predicateParameterEntity, predicateParameterKey, bindings);
            AddMemberBindings(andAlsoExpression.Right, newParameterEntity, predicateParameterEntity, predicateParameterKey, bindings);
        }
        else
        {
            throw new ArgumentException("Invalid key predicate expression is not Equal or And");
        }
    }
}
