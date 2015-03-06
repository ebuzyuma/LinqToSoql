using System;
using System.Linq.Expressions;
using System.Reflection;
using LinqToSoql.Expressions;

namespace LinqToSoql.Visitors
{
    internal class ProjectionBuilder : DbExpressionVisitor
    {
        private ParameterExpression _row;
        private static MethodInfo _miGetValue;
        private Type _sourceType;

        public ProjectionBuilder()
        {
            if (_miGetValue == null)
            {
                //_miGetValue = typeof (ProjectionRow).GetMethod("GetValue");
            }
        }

        public LambdaExpression Build(Type sourceType, Expression expression)
        {
            _sourceType = sourceType;
            _row = Expression.Parameter(sourceType, "model");
            Expression body = Visit(expression);
            return Expression.Lambda(body, _row);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            return 
                Expression.Convert(
                    Expression.Property(_row, _sourceType.GetProperty(column.Name)),
                    column.Type);
        }
    }
}