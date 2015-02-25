using System.Xml.Serialization;
using LinqToSoql.PartnerSforce;

namespace LinqToSoql.Tests.Models
{
    [XmlRoot(Namespace = "urn:partner.soap.sforce.com")]
    public class Account
    {
        public string Name { get; set; }

        public string Phone { get; set; }
    }
}