using System.Collections.Generic;
using System.Xml.Serialization;

namespace LinqToSoql.Tests.Models
{
    [XmlRoot(Namespace = "urn:partner.soap.sforce.com")]
    public class Category__c
    {
        public string Name { get; set; }
        public string Description__c { get; set; }


        [IgnoreInSoql]
        public virtual List<Product__c> Products__r { get; set; }
    }
}