using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class ReplaceParameterExpressionVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _findParameter;
    private readonly Expression _replacementExpression;

    private ReplaceParameterExpressionVisitor(
        ParameterExpression findParameter,
        Expression replacementExpression)
    {
        _findParameter = findParameter;
        _replacementExpression = replacementExpression;
    }

    public static Expression ReplaceParameter(
        Expression inputExpression,
        ParameterExpression findParameter,
        Expression replacementExpression)
    {
        var visitor = new ReplaceParameterExpressionVisitor(findParameter, replacementExpression);
        var visitedExpression = visitor.Visit(inputExpression);

        return visitedExpression;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _findParameter ? _replacementExpression : base.VisitParameter(node);
    }
}
