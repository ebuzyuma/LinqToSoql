using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToSoql.Expressions;
using LinqToSoql.Visitors;

namespace LinqToSoql.Sforce
{
    public class SforceQueryProvider : QueryProvider
    {
        private readonly SforceContext _context;

        public SforceQueryProvider(SforceContext context)
        {
            _context = context;
        }

        private TranslateResult Translate(Expression expression)
        {
            ProjectionExpression projection = expression as ProjectionExpression;
            if (projection == null)
            {
                expression = Evaluator.PartialEval(expression, CanBeEvaluatedLocally);
                expression = new QueryBinder(this).Bind(expression);
                projection = (ProjectionExpression)expression;
            }
            string commandText = new QueryFormatter().Format(projection.Source);
            var elementType = TypeSystem.GetElementType(projection.Source.From.Type);
            LambdaExpression projector = new ProjectionBuilder().Build(elementType, projection.Projector);
            return new TranslateResult { CommandText = commandText, Projector = projector };
        }

        private bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        public override string GetQueryText(Expression expression)
        {
            return Translate(expression).CommandText;
        }

        public override object Execute(Expression expression)
        {
            TranslateResult result = Translate(expression);

            Delegate projector = result.Projector.Compile();

            Type elementType = result.Projector.Parameters[0].Type;
            List<Type> parameters = new List<Type>
                                    {
                                        elementType,
                                        result.Projector.ReturnType
                                    };

            var executeMethod = _context.GetType()
                                .GetMethod("ExecuteSoqlQuery")
                                .MakeGenericMethod(parameters.ToArray());
            return executeMethod.Invoke(_context, new object[] { result.CommandText, projector });
        }
    }
}