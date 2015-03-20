using System.Xml.Serialization;

namespace LinqToSoql.Tests.Models
{
    public class Product__c
    {
        //TODO fix Nullable convertion
        public string Name { get; set; }
        public string QuantityPerUnit__c { get; set; }
        public decimal UnitPrice__c { get; set; }
        public short UnitsInStock__c { get; set; }
        public short UnitsOnOrder__c { get; set; }
        public short ReorderLevel__c { get; set; }
        public bool Discontinued__c { get; set; }

        [IgnoreInSoql]
        public virtual Category__c Category__r { get; set; }
        [IgnoreInSoql]
        public virtual Supplier__c Supplier__r { get; set; }

         
    }
}