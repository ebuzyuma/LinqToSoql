using System;
using System.Linq.Expressions;
using System.Text;
using ExpressionVisitor = LinqToSoql.Visitors.ExpressionVisitor;

namespace LinqToSoql.Visitors
{
    internal class ColumnProjector : ExpressionVisitor
    {
        private StringBuilder _stringBuilder;

        internal ColumnProjector()
        {
        }

        internal string ProjectColumns(Expression expression)
        {
            _stringBuilder = new StringBuilder();
            Visit(expression);
            return _stringBuilder.ToString();
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
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
            if (_stringBuilder.Length > 0)
                _stringBuilder.Append(", ");
            _stringBuilder.Append(p.Type.Name);
            return p;
        }
        
        /*
        protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            for (int i = 0, n = original.Count; i < n - 1; i++)
            {
                this.Visit(original[i]);
                _stringBuilder.Append(", ");
            }
            this.Visit(original.Last());
            return original;
        }

        protected override IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            for (int i = 0, n = original.Count; i < n - 1; i++)
            {
                this.VisitBinding(original[i]);
                _stringBuilder.Append(", ");
            }
            this.VisitBinding(original.Last());
            return original;
        }
        */
    }
}