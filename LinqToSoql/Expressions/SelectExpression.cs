using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace LinqToSoql.Expressions
{
    internal class SelectExpression : Expression
    {
        public string Alias { get; private set; }
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
        public Expression From { get; private set; }
        public Expression Where { get; private set; }

        public SelectExpression(Type type, string alias, IEnumerable<ColumnDeclaration> columns, Expression from, Expression where)
            : base((ExpressionType) DbExpressionType.Select, type)
        {
            Alias = alias;
            Columns = columns as ReadOnlyCollection<ColumnDeclaration> ??
                      new List<ColumnDeclaration>(columns).AsReadOnly();
            From = from;
            Where = where;
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }
}