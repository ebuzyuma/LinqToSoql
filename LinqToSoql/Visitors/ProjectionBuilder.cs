using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using LinqToSoql.Expressions;
using LinqToSoql.PartnerSforce;
using LinqToSoql;

namespace LinqToSoql.Visitors
{
    internal class ProjectionBuilder : DbExpressionVisitor
    {
        private ParameterExpression _row;
        private Type _sourceType;
        private Dictionary<string, Expression> _columns;

        public ProjectionBuilder()
        {
        }

        public LambdaExpression Build(ProjectionExpression projection)
        {
            _columns = projection.Source.Columns.ToDictionary(p => p.Name, p => p.Expression);
            _sourceType = TypeSystem.GetElementType(projection.Source.From.Type);
            _row = Expression.Parameter(_sourceType, _sourceType.Name);
            Expression body = Visit(projection.Projector);
            
            return Expression.Lambda(body, _row);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            Expression sourceColumn;
            if (_columns.TryGetValue(column.Name, out sourceColumn) && !(sourceColumn is ColumnExpression))
            {
                return Visit(sourceColumn);
            }
            return
                    Expression.Property(_row, column.Name);
        }

        private Expression Convert(Expression e, Type typeToConvert)
        {
            var changedType = Expression.Call(typeof (Convert), "ChangeType", null, 
                e, Expression.Constant(typeToConvert));
            return Expression.Convert(changedType, typeToConvert);
        }

        /*protected override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression source;
            ColumnExpression column = m.Expression as ColumnExpression;
            if (column == null)
            {
                source = Visit(m.Expression);
            }
            else
            {
                source = Expression.Property(_row, column.Name);
            }

            /*Type type = m.Type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(m.Type);
            }
            return Convert(
                Expression.Call(source, _miGetValue, Expression.Constant(m.Member.Name)),
                m.Type);
        }*/

        protected override Expression VisitProjection(ProjectionExpression projection)
        {
            SelectExpression source = (SelectExpression)Visit(projection.Source);
            Expression projector = Visit(projection.Projector);
            if (source != projection.Source || projector != projection.Projector)
            {
                return new ProjectionExpression(source, projector);
            }
            //return projection;
            return Expression.Constant(5);
        }
    }
}