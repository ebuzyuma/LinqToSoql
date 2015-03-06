using System;
using System.Linq.Expressions;

namespace LinqToSoql.Expressions
{
    internal class ColumnExpression : Expression
    {
        public string Alias { get; private set; }
        public string Name { get; private set; }

        public ColumnExpression(Type type, string alias, string name)
            : base((ExpressionType) DbExpressionType.Column, type)
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