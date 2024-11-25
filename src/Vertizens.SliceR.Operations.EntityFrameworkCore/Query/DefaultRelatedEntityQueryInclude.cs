using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class DefaultRelatedEntityQueryInclude<TMappedDomain, TEntity>(
    ITypeMapperBuilder<TMappedDomain, TEntity> _typeMapperBuilder,
    ITypeMapperExpressionBuilder<TMappedDomain, TEntity> _typeMapperExpressionBuilder,
    IEntityDefinitionResolver _entityDefinitionResolver
    ) : IRelatedEntityQueryInclude<TMappedDomain, TEntity>
{
    private class ExpressionIncludeContext
    {
        public List<string> Includes { get; set; } = [];
        public List<string> CurrentAssignSegments { get; set; } = [];
        public ParameterExpression? BlockParameterTarget { get; set; }
        public List<string> BlockParameterIncludes { get; set; } = [];
        public Expression? BinaryRightExpression { get; set; }
        public bool IsWithinBlockParameter { get; set; }
    }

    private readonly IEnumerable<string> _includes = ResolveIncludes(_typeMapperBuilder, _typeMapperExpressionBuilder, _entityDefinitionResolver);

    public IEnumerable<string> GetIncludes()
    {
        return _includes;
    }

    private static List<string> ResolveIncludes(
        ITypeMapperBuilder<TMappedDomain, TEntity> typeMapperBuilder,
        ITypeMapperExpressionBuilder<TMappedDomain, TEntity> typeMapperExpressionBuilder,
        IEntityDefinitionResolver entityDefinitionResolver)
    {
        typeMapperBuilder.Build(typeMapperExpressionBuilder);
        var mapExpression = typeMapperExpressionBuilder.Build();

        var parameterTarget = mapExpression.Parameters[1];

        var includes = new List<string>();
        if (mapExpression.Body.NodeType == ExpressionType.Block)
        {
            var context = new ExpressionIncludeContext();
            var expressionsInBlock = ((BlockExpression)mapExpression.Body).Expressions;
            foreach (var expressionInBlock in expressionsInBlock)
            {
                EvaluateExpression(entityDefinitionResolver, parameterTarget, expressionInBlock, context);
            }

            includes.AddRange(context.Includes);
        }

        var distinctIncludes = includes.Distinct().ToList();
        var compressedIncludes = distinctIncludes.Where(x => !distinctIncludes.Any(i => i.Length > x.Length && i.StartsWith(x))).ToList();

        return compressedIncludes.Distinct().ToList();
    }

    private static void EvaluateExpression(
        IEntityDefinitionResolver entityDefinitionResolver,
        ParameterExpression parameterTarget,
        Expression expression,
        ExpressionIncludeContext context)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.Assign:
                var assignExpression = (BinaryExpression)expression;
                EvaluateAssignExpression(entityDefinitionResolver, parameterTarget, assignExpression, context);
                break;
            case ExpressionType.MemberAccess:
                var memberExpression = (MemberExpression)expression;
                EvaluateMemberExpression(entityDefinitionResolver, parameterTarget, memberExpression, context);
                break;
            case ExpressionType.Block:
                var blockExpression = (BlockExpression)expression;
                var blockParameterTarget = blockExpression.Variables.FirstOrDefault() ?? context.BlockParameterTarget;

                var includes = new List<string>();
                var blockParameterIncludes = new List<string>();
                foreach (var expressionInBlock in blockExpression.Expressions)
                {
                    var blockContext = new ExpressionIncludeContext()
                    {
                        BlockParameterTarget = blockParameterTarget,
                        BlockParameterIncludes = blockParameterIncludes,
                        IsWithinBlockParameter = context.IsWithinBlockParameter || context.BlockParameterTarget != null
                    };
                    EvaluateExpression(entityDefinitionResolver, context.BlockParameterTarget ?? parameterTarget, expressionInBlock, blockContext);
                    if (blockContext.BlockParameterIncludes.Count > 0)
                    {
                        blockParameterIncludes = blockParameterIncludes.Concat(blockContext.BlockParameterIncludes).Distinct().ToList();
                    }
                    if (blockContext.Includes.Count > 0)
                    {
                        includes = includes.Concat(blockContext.Includes).Distinct().ToList();
                    }
                }
                if (blockParameterIncludes.Count > 0)
                {
                    context.BlockParameterIncludes.AddRange(blockParameterIncludes);
                }
                if (includes.Count > 0)
                {
                    context.Includes.AddRange(includes);
                }
                break;
            case ExpressionType.Conditional:
                var conditionalExpression = (ConditionalExpression)expression;
                var conditionContext = new ExpressionIncludeContext
                {
                    Includes = context.Includes,
                    BlockParameterIncludes = context.BlockParameterIncludes,
                    BlockParameterTarget = context.BlockParameterTarget,
                    IsWithinBlockParameter = context.IsWithinBlockParameter
                };
                EvaluateExpression(entityDefinitionResolver, parameterTarget, conditionalExpression.IfTrue, conditionContext);

                conditionContext = new ExpressionIncludeContext
                {
                    Includes = context.Includes,
                    BlockParameterIncludes = context.BlockParameterIncludes,
                    BlockParameterTarget = context.BlockParameterTarget,
                    IsWithinBlockParameter = context.IsWithinBlockParameter
                };
                EvaluateExpression(entityDefinitionResolver, parameterTarget, conditionalExpression.IfFalse, conditionContext);
                break;
        }
    }

    private static void EvaluateAssignExpression(IEntityDefinitionResolver entityDefinitionResolver, ParameterExpression parameterTarget, BinaryExpression binaryExpression, ExpressionIncludeContext context)
    {
        if (binaryExpression.Left is MemberExpression memberExpression)
        {
            var assignContext = new ExpressionIncludeContext
            {
                Includes = context.Includes,
                CurrentAssignSegments = context.CurrentAssignSegments,
                BlockParameterTarget = context.BlockParameterTarget,
                BlockParameterIncludes = context.BlockParameterIncludes,
                IsWithinBlockParameter = context.IsWithinBlockParameter,
                BinaryRightExpression = binaryExpression.Right
            };
            EvaluateMemberExpression(entityDefinitionResolver, parameterTarget, memberExpression, assignContext);
        }
    }

    private static void EvaluateMemberExpression(IEntityDefinitionResolver entityDefinitionResolver, ParameterExpression parameterTarget, MemberExpression memberExpression, ExpressionIncludeContext context)
    {
        if (memberExpression.Member.MemberType == MemberTypes.Property)
        {
            var propertyInfo = (PropertyInfo)memberExpression.Member;

            if (IsEntity(entityDefinitionResolver, propertyInfo.PropertyType))
            {
                context.CurrentAssignSegments.Insert(0, propertyInfo.Name);
            }
            else
            {
                context.CurrentAssignSegments.Clear();
            }

            if (memberExpression.Expression != null)
            {
                if (memberExpression.Expression != parameterTarget)
                {
                    EvaluateExpression(entityDefinitionResolver, parameterTarget, memberExpression.Expression, context);
                }
                else if (memberExpression.Expression == parameterTarget)
                {
                    if (context.CurrentAssignSegments.Count > 0)
                    {
                        var includes = context.Includes;
                        if (context.IsWithinBlockParameter)
                        {
                            includes = context.BlockParameterIncludes;
                        }

                        var include = string.Join('.', context.CurrentAssignSegments);
                        if (context.BinaryRightExpression == context.BlockParameterTarget)
                        {
                            foreach (var blockParameterInclude in context.BlockParameterIncludes)
                            {
                                includes.Add(include + "." + blockParameterInclude);
                            }
                        }
                        else
                        {
                            includes.Add(include);
                        }
                    }
                }
            }
        }
    }

    private static bool IsEntity(IEntityDefinitionResolver entityDefinitionResolver, Type propertyType)
    {
        var propertyEntity = entityDefinitionResolver.Get(propertyType);
        if (propertyEntity == null && propertyType != typeof(string) && propertyType.IsAssignableTo(typeof(IEnumerable)))
        {
            var enumerableInterface = propertyType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerableInterface != null)
            {
                var enumerableType = enumerableInterface.GetGenericArguments()[0];
                propertyEntity = entityDefinitionResolver.Get(enumerableType);
            }
        }

        return propertyEntity != null;
    }
}
