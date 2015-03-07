using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using LinqToSoql.Expressions;

namespace LinqToSoql.Visitors
{
    internal class UnusedColumnRemover : DbExpressionVisitor
    {
        Dictionary<string, HashSet<string>> _allColumnsUsed;

        public Expression Remove(Expression expression)
        {
            _allColumnsUsed = new Dictionary<string, HashSet<string>>();
            return Visit(expression);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            HashSet<string> columns;
            if (!_allColumnsUsed.TryGetValue(column.Alias, out columns))
            {
                columns = new HashSet<string>();
                _allColumnsUsed.Add(column.Alias, columns);
            }
            columns.Add(column.Name);
            return column;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            // visit column projection first
            ReadOnlyCollection<ColumnDeclaration> columns = select.Columns;

            HashSet<string> columnsUsed;
            if (_allColumnsUsed.TryGetValue(select.Alias, out columnsUsed))
            {
                List<ColumnDeclaration> alternate = null;
                for (int i = 0, n = select.Columns.Count; i < n; i++)
                {
                    ColumnDeclaration decl = select.Columns[i];
                    if (!columnsUsed.Contains(decl.Name))
                    {
                        decl = null;  // null means it gets omitted
                    }
                    else
                    {
                        Expression expr = Visit(decl.Expression);
                        if (expr != decl.Expression)
                        {
                            decl = new ColumnDeclaration(decl.Name, decl.Expression);
                        }
                    }
                    if (decl != select.Columns[i] && alternate == null)
                    {
                        alternate = new List<ColumnDeclaration>();
                        for (int j = 0; j < i; j++)
                        {
                            alternate.Add(select.Columns[j]);
                        }
                    }
                    if (decl != null && alternate != null)
                    {
                        alternate.Add(decl);
                    }
                }
                if (alternate != null)
                {
                    columns = alternate.AsReadOnly();
                }
            }

            //ReadOnlyCollection<OrderExpression> orderbys = this.VisitOrderBy(select.OrderBy);
            Expression where = Visit(select.Where);
            Expression from = Visit(select.From);

            if (columns != select.Columns || /*orderbys != select.OrderBy ||*/ where != select.Where || from != select.From)
            {
                return new SelectExpression(select.Type, select.Alias, columns, from, where); //, orderbys);
            }

            return select;
        }

        protected override Expression VisitProjection(ProjectionExpression projection)
        {
            // visit mapping in reverse order
            Expression projector = Visit(projection.Projector);
            SelectExpression source = (SelectExpression)Visit(projection.Source);
            if (projector != projection.Projector || source != projection.Source)
            {
                return new ProjectionExpression(source, projector);
            }
            return projection;
        }

        /*
        protected override Expression VisitJoin(JoinExpression join)
        {
            // visit join in reverse order
            Expression condition = this.Visit(join.Condition);
            Expression right = this.VisitSource(join.Right);
            Expression left = this.VisitSource(join.Left);
            if (left != join.Left || right != join.Right || condition != join.Condition)
            {
                return new JoinExpression(join.Type, join.Join, left, right, condition);
            }
            return join;
        }*/
    }
}