using System.Xml.Serialization;
using LinqToSoql.PartnerSforce;

namespace LinqToSoql.Tests.Models
{
    public class Account
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }
    }
}