using System.Linq.Expressions;

namespace LinqToSoql
{
    internal class TranslateResult
    {
        public string CommandText { get; set; }
        public LambdaExpression Projector { get; set; }
    }
}