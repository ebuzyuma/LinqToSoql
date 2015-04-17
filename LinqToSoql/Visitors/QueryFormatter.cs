using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LinqToSoql.Expressions;

namespace LinqToSoql.Visitors
{
    internal class QueryFormatter : DbExpressionVisitor
    {
        private StringBuilder _stringBuilder;
        private int _indent = 2;
        private int _depth;

        public QueryFormatter()
        {
        }

        public string Format(Expression expression)
        {
            _stringBuilder = new StringBuilder();
            Visit(expression);
            return _stringBuilder.ToString();
        }

        protected enum Identation
        {
            Same,
            Inner,
            Outer
        }

        public int IdentationWidth { get { return _indent; } set { _indent = value; } }

        private void AppendNewLine(Identation style)
        {
            _stringBuilder.AppendLine();
            if (style == Identation.Inner)
            {
                _depth++;
            }
            else if (style == Identation.Outer)
            {
                _depth--;
                Debug.Assert(_depth >= 0);
            }
            for (int i = 0, n = _depth * _indent; i < n; i++)
            {
                _stringBuilder.Append(" ");
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.Name == "Like")
            {
                Visit(m.Arguments[0]);
                _stringBuilder.Append(" LIKE ");
                Visit(m.Arguments[1]);
                return m;
            }
            if (m.Method.DeclaringType == typeof (string))
            {
                Visit(m.Object);
                _stringBuilder.Append(" LIKE ");

                ConstantExpression ce = m.Arguments[0] as ConstantExpression;
                if (ce == null)
                    throw new ArgumentException("soql like pattern should be a constant value");
                switch (m.Method.Name)
                {
                    case "StartsWith": 
                        ce = Expression.Constant(ce.Value + "%");
                        break;
                    case "EndsWith":
                        ce = Expression.Constant("%" + ce.Value);
                        break;
                    case "Contains":
                        ce = Expression.Constant("%" + ce.Value + "%");
                        break;
                }
                return Visit(ce);
            }

            if (m.Method.Name == "Contains")
            {
                Visit(m.Arguments[0]);

                _stringBuilder.Append(" IN (");
                AppendNewLine(Identation.Inner);
                Visit(m.Object);
                AppendNewLine(Identation.Outer);
                _stringBuilder.Append(")");
                
                return m;
            }

            throw new NotSupportedException(String.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    var m = u.Operand as MethodCallExpression;
                    if (m != null && m.Method.Name == "Contains") 
                    {
                        Visit(m.Arguments[0]);

                        _stringBuilder.Append(" NOT IN (");
                        AppendNewLine(Identation.Inner);
                        Visit(m.Object);
                        AppendNewLine(Identation.Outer);
                        _stringBuilder.Append(")");
                        return m;
                    }
                    _stringBuilder.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(String.Format("The unary operand '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _stringBuilder.Append("(");
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
                default:
                    throw new NotSupportedException(String.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            Visit(b.Right);
            _stringBuilder.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
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
                        var collection = c.Value as IEnumerable<object>;
                        if (collection != null)
                        {
                            var enumerator = collection.GetEnumerator();
                            if (enumerator.MoveNext())                                
                                _stringBuilder.AppendFormat("'{0}'", enumerator.Current);
                            while (enumerator.MoveNext())
                            {
                                _stringBuilder.AppendFormat(", '{0}'", enumerator.Current);
                            }
                            break;
                        }
                        else
                            throw new NotSupportedException(String.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        _stringBuilder.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (!String.IsNullOrEmpty(column.Alias))
            {
                _stringBuilder.Append(column.Alias);
                _stringBuilder.Append(".");
            }
            _stringBuilder.Append(column.Name);
            return column;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            _stringBuilder.Append("SELECT ");
            for (int i = 0, n = select.Columns.Count; i < n; i++)
            {
                ColumnDeclaration column = select.Columns[i];
                if (i > 0)
                {
                    _stringBuilder.Append(", ");
                }

                Visit(column.Expression);
                //ColumnExpression cex = Visit(column.Expression) as ColumnExpression;
                //if (cex == null || cex.Name != select.Columns[i].Name)
                //{
                    //_stringBuilder.Append(" AS ");
                    //_stringBuilder.Append(column.Name);
                //}

            }
            if (select.From != null)
            {
                AppendNewLine(Identation.Same);
                _stringBuilder.Append("FROM ");
                VisitSource(select.From);
                //TODO update DbExpressionTypes for handing column as table
                if (select.From is ColumnExpression)
                {
                    _stringBuilder.Append(" AS ");
                    _stringBuilder.Append(((ColumnExpression)select.Columns.First().Expression).Alias);
                }
            }
            if (select.Where != null)
            {
                AppendNewLine(Identation.Same);
                _stringBuilder.Append("WHERE ");
                Visit(select.Where);
            }
            return select;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            Visit(m.Expression);
            _stringBuilder.Append(".");
            _stringBuilder.Append(m.Member.Name);
            return m;
        }

        protected override Expression VisitSource(Expression source)
        {
            switch ((DbExpressionType)source.NodeType)
            {
                case DbExpressionType.Table:
                    TableExpression table = (TableExpression) source;
                    _stringBuilder.Append(table.Name);
                    _stringBuilder.Append(" AS ");
                    _stringBuilder.Append(table.Alias);
                    break;
                case DbExpressionType.Select:
                    SelectExpression select = (SelectExpression) source;
                    _stringBuilder.Append("(");
                    AppendNewLine(Identation.Inner);
                    Visit(select);
                    AppendNewLine(Identation.Outer);
                    _stringBuilder.Append(")");
                    break;
                case DbExpressionType.Column:
                    Visit(source);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Select source '{0}' is not a valid type", source.NodeType));
            }
            return source;
        }

        protected override Expression VisitProjection(ProjectionExpression projection)
        {
            VisitSource(projection.Source);
            return projection;
        }
    }
}