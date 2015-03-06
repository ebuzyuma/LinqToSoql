using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LinqToSoql.Expressions
{
    internal class ProjectionExpression : Expression
    {
        public SelectExpression Source { get; private set; }
        public Expression Projector { get; private set; }

        public ProjectionExpression(SelectExpression source, Expression projector)
            : base((ExpressionType) DbExpressionType.Projection,projector.Type)
        {
            Source = source;
            Projector = projector;
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }
}