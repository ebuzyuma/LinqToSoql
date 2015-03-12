using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using LinqToSoql.Expressions;
using ExpressionVisitor = LinqToSoql.Visitors.ExpressionVisitor;

namespace LinqToSoql.Visitors
{
    internal sealed class ProjectedColumns
    {
        public Expression Projector { get; private set; }
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }

        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Projector = projector;
            Columns = columns;
        }
    }

    internal class ColumnProjector : DbExpressionVisitor
    {
        private readonly Nominator _nominator;
        private Dictionary<ColumnExpression, ColumnExpression> _map;
        private List<ColumnDeclaration> _columns;
        private HashSet<string> _columnNames;
        private HashSet<Expression> _candidates;
        private string _existingAlias;
        private string _newAlias;
        private int _iColumn;

        public ColumnProjector(Func<Expression, bool> fnCanBeColumn)
        {
            _nominator = new Nominator(fnCanBeColumn);
        }

        public ProjectedColumns ProjectColumns(Expression expression, string newAlias, string existingAlias)
        {
            _map = new Dictionary<ColumnExpression, ColumnExpression>();
            _columns = new List<ColumnDeclaration>();
            _columnNames = new HashSet<string>();
            _newAlias = newAlias;
            _existingAlias = existingAlias;
            _candidates = _nominator.Nominate(expression);
            return new ProjectedColumns(Visit(expression), _columns.AsReadOnly());
        }

        protected override Expression Visit(Expression expression)
        {
            if (_candidates.Contains(expression))
            {
                if (expression.NodeType == (ExpressionType) DbExpressionType.Column)
                {
                    ColumnExpression column = (ColumnExpression) expression;
                    ColumnExpression mapped;
                    if (_map.TryGetValue(column, out mapped))
                    {
                        return mapped;
                    }
                    if (_existingAlias == column.Alias)
                    {
                        string columnName = GetUniqueColumnName(column.Name);
                        _columns.Add(new ColumnDeclaration(columnName, column));
                        mapped = new ColumnExpression(column.Type, _newAlias, columnName);
                        _map[column] = mapped;
                        _columnNames.Add(columnName);
                        return mapped;
                    }
                    // must be referring to outer scope
                    return column;
                }
                else
                {
                    string columnName = GetNextColumnName();
                    _columns.Add(new ColumnDeclaration(columnName, expression));
                    return new ColumnExpression(expression.Type, _newAlias, columnName);
                }
            }
            else
            {
                return base.Visit(expression);
            }
        }

        private bool IsColumnNameInUse(string name)
        {
            return _columnNames.Contains(name);
        }

        private string GetUniqueColumnName(string name)
        {
            string baseName = name;
            int suffix = 1;
            while (IsColumnNameInUse(name))
            {
                name = baseName + (suffix++);
            }
            return name;
        }

        private string GetNextColumnName()
        {
            return GetUniqueColumnName("c" + (_iColumn++));
        }

        private class Nominator : DbExpressionVisitor
        {
            private readonly Func<Expression, bool> _fnCanBeColumn;
            private bool _isBlocked;
            private HashSet<Expression> _candidates;

            internal Nominator(Func<Expression, bool> fnCanBeColumn)
            {
                _fnCanBeColumn = fnCanBeColumn;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                _candidates = new HashSet<Expression>();
                _isBlocked = false;
                Visit(expression);
                return _candidates;
            }

            protected override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveIsBlocked = _isBlocked;
                    _isBlocked = false;
                    base.Visit(expression);
                    if (!_isBlocked)
                    {
                        if (_fnCanBeColumn(expression))
                        {
                            _candidates.Add(expression);
                        }
                        else
                        {
                            _isBlocked = true;
                        }
                    }
                    _isBlocked |= saveIsBlocked;
                }
                return expression;
            }

            protected override Expression VisitMemberAccess(MemberExpression m)
            {
                if (_fnCanBeColumn(m.Expression))
                {
                    return m;
                }
                return base.VisitMemberAccess(m);
            }
        }
    }
}