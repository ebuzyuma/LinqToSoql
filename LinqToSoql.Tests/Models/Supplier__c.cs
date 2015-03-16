using System;
using System.Collections.Generic;

namespace LinqToSoql.Tests.Models
{
    public class Supplier__c
    {
        public string Name { get; set; }
        public string ContactName__c { get; set; }
        public string ContactTitle__c { get; set; }
        public string Address__c { get; set; }
        public string City__c { get; set; }
        public string Region__c { get; set; }
        public string PostalCode__c { get; set; }
        public string Country__c { get; set; }
        public string Phone__c { get; set; }
        public string Fax__c { get; set; }
        public string HomePage__c { get; set; }


        public List<Product__c> Products__r { get; set; }         
    }
}