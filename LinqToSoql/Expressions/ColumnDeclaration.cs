using System.Linq.Expressions;

namespace LinqToSoql.Expressions
{
    internal class ColumnDeclaration
    {
        public string Name { get; private set; }
        public Expression Expression { get; private set; }

        public ColumnDeclaration(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
        }
    }
}