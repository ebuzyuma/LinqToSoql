using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
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
            return Enumerable.Select(res, projector);
        }

        private T Map<T>(sObject obj) where T : class, new()
        {
            var xmlDoc = obj.Any.Select(RemoveSForcePrefixAndAttributes);
            string root = obj.type;
            string inner = String.Join("\n", xmlDoc.Select(p => p.OuterXml));
            string xml = String.Format("<{0}>{1}</{0}>", root, inner);

            var serializer = new XmlSerializer(typeof (T));
            var reader = new StringReader(xml);
            return serializer.Deserialize(reader) as T;
        }

        private XmlNode RemoveSForcePrefixAndAttributes(XmlNode obj)
        {
            XmlElement res = new XmlDocument().CreateElement(obj.LocalName);
            foreach (object node in obj.ChildNodes)
            {
                var xmlNode = (XmlNode) node;

                // current node should be replaced with corresponding object
                if (xmlNode.LocalName == "records")
                {
                    var enumerator = xmlNode.ChildNodes.GetEnumerator();
                    while (enumerator.MoveNext() && ((XmlNode) enumerator.Current).LocalName != "type");

                    string type = ((XmlNode) enumerator.Current).InnerText;
                    XmlElement elem = new XmlDocument().CreateElement(type);

                    enumerator.Reset();
                    while(enumerator.MoveNext())
                    {
                        var cur = RemoveSForcePrefixAndAttributes((XmlNode) enumerator.Current);
                        cur = elem.OwnerDocument.ImportNode(cur, true);
                        elem.AppendChild(cur);
                    }
                    xmlNode = elem;
                }
                else if(xmlNode is XmlElement)
                    xmlNode = RemoveSForcePrefixAndAttributes(xmlNode);

                XmlNode importNode = res.OwnerDocument.ImportNode(xmlNode, true);
                res.AppendChild(importNode);
            }
            return res;
        }

        public void Dispose()
        {
            _binding.logout();
        }
    }
}