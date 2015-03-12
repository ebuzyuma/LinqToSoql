using System.Collections.Generic;
using System.Linq;
using System.Xml;
using LinqToSoql.PartnerSforce;

namespace LinqToSoql.PartnerSforce 
{
    public partial class sObject
    {
        public object GetValue(string name)
        {
            return Any.First(p => p.Name == "sf:" + name).InnerText;
        }

        public sObject GetProperty(string name)
        {
            XmlElement[] elems = Any.First(p => p.Name == "sf:" + name).ChildNodes.Cast<XmlElement>().ToArray();
            return new sObject {Any = elems};
        }
    }
}