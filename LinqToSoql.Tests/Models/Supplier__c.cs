using System;
using System.Collections.Generic;

namespace LinqToSoql.Tests.Models
{
    public class Supplier__c
    {
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string HomePage { get; set; }


        public List<Product__c> Products__r { get; set; }         
    }
}