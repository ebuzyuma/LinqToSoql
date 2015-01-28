using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToSoql.Visitors;

namespace LinqToSoql
{
    public class SforceQueryProvider : QueryProvider
    {
        private readonly ISforceContext _context;

        public SforceQueryProvider(ISforceContext context)
        {
            _context = context;
        }

        private TranslateResult Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator().Translate(expression);
        }

        public override string GetQueryText(Expression expression)
        {
            return Translate(expression).Query;
        }

        public override object Execute(Expression expression)
        {
            TranslateResult result = Translate(expression);

            List<Type> parameters = new List<Type>(result.Projector.Parameters.Select(p =>p.Type));
            parameters.Add(result.Projector.ReturnType);

            var executeMethod = _context.GetType()
                                .GetMethod("ExecuteSoqlQuery")
                                .MakeGenericMethod(parameters.ToArray());
            return executeMethod.Invoke(_context, new object[] {result.Query, result.Projector.Compile()});
        }
    }
}