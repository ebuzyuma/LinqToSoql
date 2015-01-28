using System;
using System.Collections.Generic;

namespace LinqToSoql
{
    public interface ISforceContext
    {
        IEnumerable<TResult> ExecuteSoqlQuery<T, TResult>(string query, Func<T, TResult> projector);

        Query<TModel> GetTable<TModel>();

    }
}