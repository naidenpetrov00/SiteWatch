using System.Linq.Expressions;

namespace Application.SeedWork.Queries;

internal sealed class ReplaceParameterVisitor(ParameterExpression from, Expression to)
    : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == from ? to : base.VisitParameter(node);
    }
}

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right
    )
    {
        var parameter = left.Parameters[0];
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(
            right.Body
        )!;

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left.Body, rightBody),
            parameter
        );
    }
}
