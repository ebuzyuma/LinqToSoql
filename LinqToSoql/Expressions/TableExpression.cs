using System;
using System.Linq.Expressions;

namespace LinqToSoql.Expressions
{
    internal class TableExpression : Expression
    {
        public string Alias { get; private set; }
        public string Name { get; private set; }

        public TableExpression(Type type, string alias, string name) : base((ExpressionType) DbExpressionType.Table, type)
        {
            Alias = alias;
            Name = name;
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }
}