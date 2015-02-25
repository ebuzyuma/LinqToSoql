using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using LinqToSoql.PartnerSforce;

namespace LinqToSoql.Sforce
{
    public class SforceContext : IDisposable
    {
        private readonly SforceService _binding;
        private readonly SforceQueryProvider _queryProvider;

        public SforceContext(string username, string password, string token)
        {
            try
            {
                _binding = Login(username, password, token);
            }
            catch (AuthenticationException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AuthenticationException("An error occured while trying to login", e);
            }

            _queryProvider = new SforceQueryProvider(this);
        }

        private SforceService Login(string username, string password, string token)
        {
            var binding = new SforceService();
            binding.Timeout = 60000; // 1 min

            LoginResult lr;
            lr = binding.login(username, password + token); // can throw SoapExeption

            if (lr.passwordExpired)
            {
                throw new AuthenticationException("Password Expired!");
            }

            binding.Url = lr.serverUrl;
            binding.SessionHeaderValue = new SessionHeader();
            binding.SessionHeaderValue.sessionId = lr.sessionId;

            return binding;
        }

        public Query<TModel> GetTable<TModel>()
        {
            return new Query<TModel>(_queryProvider);
        }

        public IEnumerable<TResult> ExecuteSoqlQuery<TModel, TResult>(string query, Func<TModel, TResult> projector) where TModel : class, new()
        {
            //TODO add Enumerator
            QueryResult qr = _binding.queryAll(query);
            var res = qr.records.Select(Map<TModel>);
            return res.Select(projector);
        }

        private T Map<T>(sObject obj) where T : class, new()
        {
            string xmlns = "xmlns:sf='urn:partner.soap.sforce.com'";
            string xmlnsPattern = "xmlns:sf=['\"][a-zA-Z.:]+['\"]";
            string root = obj.type;
            string inner = String.Join("\n", obj.Any.Select(p => Regex.Replace(p.OuterXml, xmlnsPattern, String.Empty)));
            string xml = String.Format("<sf:{0} {1}>{2}</sf:{3}>", root, xmlns, inner, root);

            var serializer = new XmlSerializer(typeof (T));
            var reader = new StringReader(xml);
            return serializer.Deserialize(reader) as T;
        }

        public void Dispose()
        {
            _binding.logout();
        }
    }
}