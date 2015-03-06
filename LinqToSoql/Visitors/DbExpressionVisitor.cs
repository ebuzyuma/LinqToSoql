using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using LinqToSoql.Expressions;

namespace LinqToSoql.Visitors
{
    internal class DbExpressionVisitor : ExpressionVisitor
    {
        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            switch ((DbExpressionType) exp.NodeType)
            {
                case DbExpressionType.Table:
                    return VisitTable((TableExpression) exp);
                case DbExpressionType.Column:
                    return VisitColumn((ColumnExpression) exp);
                case DbExpressionType.Select:
                    return VisitSelect((SelectExpression) exp);
                case DbExpressionType.Projection:
                    return VisitProjection((ProjectionExpression) exp);
            }
            return base.Visit(exp);
        }

        protected virtual Expression VisitTable(TableExpression table)
        {
            return table;
        }
        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            return column;
        }

        protected virtual Expression VisitProjection(ProjectionExpression projection)
        {
            SelectExpression source = (SelectExpression) Visit(projection.Source);
            Expression projector = Visit(projection.Projector);
            if (source != projection.Source || projector != projection.Projector)
            {
                return new ProjectionExpression(source, projector);
            }
            return projection;
        }

        protected virtual Expression VisitSelect(SelectExpression select)
        {
            Expression from = VisitSource(select.From);
            Expression where = Visit(select.Where);
            ReadOnlyCollection<ColumnDeclaration> columns = VisitColumnDeclarations(select.Columns);
            if (from != select.From || where != select.Where || columns != select.Columns)
            {
                return new SelectExpression(select.Type, select.Alias, columns, from, where);
            }
            return select;
        }

        protected virtual Expression VisitSource(Expression source)
        {
            return Visit(source);
        }

        protected virtual ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            List<ColumnDeclaration> alternative = null;
            for (int i = 0, n = columns.Count; i < n; i++)
            {
                ColumnDeclaration column = columns[i];
                Expression e = Visit(column.Expression);
                if (alternative == null && e != column.Expression)
                {
                    alternative = columns.Take(i).ToList();
                }
                if (alternative != null)
                {
                    alternative.Add(new ColumnDeclaration(column.Name, e));
                }
            }
            if (alternative != null)
            {
                return alternative.AsReadOnly();
            }
            return columns;
        }

    }
}