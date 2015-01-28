using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinqToSoql.EnterpriseDemo.SforceEnterpriseService;

namespace LinqToSoql.EnterpriseDemo.Sforce
{
    public class SforceContext : ISforceContext
    {
        private readonly SessionHeader _header;
        private readonly SoapClient _client;
        private readonly SforceQueryProvider _queryProvider;

        public SforceContext(SessionHeader header, SoapClient client)
        {
            _header = header;
            _client = client;
            _queryProvider = new SforceQueryProvider(this);
        }

        public Query<TModel> GetTable<TModel>()
        {
            return new Query<TModel>(_queryProvider);
        }

        public IEnumerable<TResult> ExecuteSoqlQuery<T, TResult>(string query, Func<T, TResult> projector)
        {
            //TODO add Enumerator
            QueryResult qr;
            // TODO investigate LimitInfo[], returned after performing query
            _client.query(
                _header, // session header
                null, // query options
                null, // mru options
                null, // package version header
                query, // query string
                out qr
                );

            bool done = false;
            var queryResults = new List<T>();

            if (qr.size > 0)
            {
                while (!done)
                {
                    queryResults.AddRange(qr.records.Cast<T>());
                    if (qr.done)
                    {
                        done = true;
                    }
                    else
                    {
                        _client.queryMore(
                            _header, // session header
                            null, // query options
                            qr.queryLocator, // query locator
                            out qr
                            );
                    }
                }
            }
            
            return queryResults.Select(projector);
        }

        public IEnumerable<string> GetColumnsFor<T>()
        {
            DescribeSObjectResult des;
            _client.describeSObject(_header, null, null, typeof(T).Name, out des);
            return des.fields.Select(p => p.name);
        }

        public IEnumerable<string> GetColumnsFor(Type elementType)
        {
            MethodInfo methodInfo = typeof (SforceContext).GetMethods()
                                    .First(p => p.Name=="GetColumnsFor" && p.GetParameters().Length == 0)
                                    .MakeGenericMethod(elementType);
            
            return (IEnumerable<string>) methodInfo.Invoke(this, null);
        }
    }
}