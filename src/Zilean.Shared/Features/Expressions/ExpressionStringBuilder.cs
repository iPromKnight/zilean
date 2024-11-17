namespace Zilean.Shared.Features.Expressions;

public class ExpressionStringBuilder : ExpressionVisitor
{
    private readonly StringBuilder _sb = new();

    public override Expression Visit(Expression? node)
    {
        if (node == null)
        {
            return null;
        }

        switch (node.NodeType)
        {
            case ExpressionType.Lambda:
                var lambda = (LambdaExpression)node;
                Visit(lambda.Body);
                break;
            case ExpressionType.MemberAccess:
                var member = (MemberExpression)node;
                Visit(member.Expression);
                _sb.Append($".{member.Member.Name}");
                break;
            case ExpressionType.Constant:
                var constant = (ConstantExpression)node;
                _sb.Append(constant.Value);
                break;
            case ExpressionType.Equal:
                var binaryEqual = (BinaryExpression)node;
                Visit(binaryEqual.Left);
                _sb.Append(" == ");
                Visit(binaryEqual.Right);
                break;
            case ExpressionType.NotEqual:
                var binaryNotEqual = (BinaryExpression)node;
                Visit(binaryNotEqual.Left);
                _sb.Append(" != ");
                Visit(binaryNotEqual.Right);
                break;
            case ExpressionType.AndAlso:
                var binaryAnd = (BinaryExpression)node;
                Visit(binaryAnd.Left);
                _sb.Append(" && ");
                Visit(binaryAnd.Right);
                break;
            case ExpressionType.OrElse:
                var binaryOr = (BinaryExpression)node;
                Visit(binaryOr.Left);
                _sb.Append(" || ");
                Visit(binaryOr.Right);
                break;
            case ExpressionType.Call:
                var methodCall = (MethodCallExpression)node;
                Visit(methodCall.Object);
                _sb.Append($".{methodCall.Method.Name}(");
                for (int i = 0; i < methodCall.Arguments.Count; i++)
                {
                    if (i > 0)
                    {
                        _sb.Append(", ");
                    }

                    Visit(methodCall.Arguments[i]);
                }

                _sb.Append(")");
                break;
            default:
                _sb.Append(node);
                break;
        }

        return node;
    }

    public override string ToString() => _sb.ToString();
}
