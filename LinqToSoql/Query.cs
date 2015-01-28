using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToSoql
{
    public class Query<T> : 
        IQueryable<T>, IQueryable, 
        IEnumerable<T>, IEnumerable,
        IOrderedQueryable<T>, IOrderedQueryable
    {

        private readonly QueryProvider _provider;
        private readonly Expression _expression;

        public Query(QueryProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            _provider = provider;
            _expression = Expression.Constant(this);
        }

        public Query(QueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if(!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            _expression = expression;
            _provider = provider;
        }

        Expression IQueryable.Expression
        {
            get { return _expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof (T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        } 

        public IEnumerator<T> GetEnumerator()
        {
            var result = _provider.Execute(_expression);
            
            return ((IEnumerable<T>) result).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
           // return ((IEnumerable) _provider.Execute(_expression)).GetEnumerator();
            return GetEnumerator();
        }

        public override string ToString()
        {
            return _provider.GetQueryText(_expression);
        }

    }
}