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
                Expression modified = Evaluator.PartialEval(expression, CanBeEvaluatedLocally);
                modified = new QueryBinder(this).Bind(modified);
                //expression = new OrderByRewriter().Rewrite(expression);
                modified = new UnusedColumnRemover().Remove(modified);
                modified = new RedundantSubqueryRemover().Remove(modified);
                projection = (ProjectionExpression)modified;
            }
            string commandText = new QueryFormatter().Format(projection.Source);
            //LambdaExpression projector = new ProjectionBuilder().Build(projection);
            LambdaExpression projector = ((UnaryExpression)((MethodCallExpression)expression).Arguments[1]).Operand as LambdaExpression;
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
            
            Type modelType = result.Projector.Parameters[0].Type;
            Type resultType = result.Projector.ReturnType;

            var executeMethod = _context.GetType()
                                .GetMethod("ExecuteSoqlQuery")
                                .MakeGenericMethod(modelType, resultType);
            return executeMethod.Invoke(_context, new object[] { result.CommandText, projector });
        }
    }
}