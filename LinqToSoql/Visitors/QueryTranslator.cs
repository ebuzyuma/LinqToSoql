using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSoql.Visitors
{
    internal class TranslateResult
    {
        internal string Query;
        internal LambdaExpression Projector;
    }

    internal class QueryTranslator : ExpressionVisitor
    {
        private StringBuilder _stringBuilder;
        private LambdaExpression _selectLambda;

        internal QueryTranslator()
        {
        }

        internal TranslateResult Translate(Expression expression)
        {
            _stringBuilder = new StringBuilder();
            Visit(expression);
            return new TranslateResult
                   {
                       Query = _stringBuilder.ToString(),
                       Projector = _selectLambda
                   };
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof (Queryable))
            {
                if (m.Method.Name == "Select")
                {
                    _stringBuilder.Append("SELECT ");

                    var lambda = (LambdaExpression) StripQuotes(m.Arguments[1]);
                    string projection = new ColumnProjector().ProjectColumns(lambda.Body);
                    _selectLambda = lambda;
                    _stringBuilder.Append(projection);

                    if (m.Arguments[0].NodeType == ExpressionType.Constant)
                        _stringBuilder.Append(" FROM ");

                    this.Visit(m.Arguments[0]);

                    return m;
                }
                if (m.Method.DeclaringType == typeof (Queryable) && m.Method.Name == "Where")
                {
                    _stringBuilder.Append(" FROM ");

                    this.Visit(m.Arguments[0]);

                    _stringBuilder.Append(" WHERE ");

                    this.Visit(m.Arguments[1]);

                    return m;
                }
            }

            throw new NotSupportedException(String.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    _stringBuilder.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                case ExpressionType.Quote:
                    Expression operand = this.Visit(u.Operand);
                    if (operand != u.Operand)
                        return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    _stringBuilder.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    _stringBuilder.Append(" != ");
                    break;
                case ExpressionType.LessThan:
                    _stringBuilder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _stringBuilder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _stringBuilder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _stringBuilder.Append(" >= ");
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _stringBuilder.Append(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _stringBuilder.Append(" OR ");
                    break;
            }
            Visit(b.Right);
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;
            if (q != null)
            {                
                //_stringBuilder.Append("SELECT ");
                //_stringBuilder.Append(String.Join(", ", _context.GetColumnsFor(q.ElementType)));                
                //_stringBuilder.Append(" FROM ");

                _stringBuilder.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                _stringBuilder.Append("null");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.String:
                        _stringBuilder.Append("'");
                        _stringBuilder.Append(c.Value);
                        _stringBuilder.Append("'");
                        break;
                    case TypeCode.DateTime:
                        // according to http://www.salesforce.com/us/developer/docs/soql_sosl/
                        // Salesforce Object Query Language (SOQL) -> SOQL SELECT Syntax -> fieldExpression Syntax -> Date Formats and Date Literals
                        _stringBuilder.Append(((DateTime)c.Value).ToUniversalTime().ToString("yyyy-MM-dd")); //TODO differentiate date(e g 2000-12-12) and date/time(yyyy-MM-ddThh:mm:ssZ, e g 2001-11-11T10:10:15Z) format for soql
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        _stringBuilder.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            //TODO implement string -> LIKE in soql 
            Visit(m.Expression);
            Type type = m.Member.ReflectedType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
                return m;

            _stringBuilder.Append(".");
            _stringBuilder.Append(m.Member.Name);

            return m;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            _stringBuilder.Append(p.Type.Name);
            return p;
        }
    }
}