using System;
using System.Linq;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;
using LinqToSoql.Tests.Utils;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace LinqToSoql.Tests
{
    [TestFixture]
    public class WhereTests : TestsBase
    {
        [Test]
        public void Simple()
        {
            var linq =
                from p in SforceManager.Context.GetTable<Product__c>()
                where p.UnitPrice__c > 40
                select p.UnitPrice__c;

            string expected = "SELECT t0.UnitPrice__c FROM Product__c AS t0 WHERE (t0.UnitPrice__c > 40)";
            string actual = linq.ToString();

            Assert.That(actual, EqualTo(expected));
            
            var res = linq.ToList();

            Assert.That(res, Is.All.GreaterThan(40));
        }

        [Test]
        public void IntParameter()
        {
            int value = 40;
            var linq =
                from p in SforceManager.Context.GetTable<Product__c>()
                where p.UnitPrice__c > value
                select p.UnitPrice__c;

            string expected = String.Format("SELECT t0.UnitPrice__c FROM Product__c AS t0 WHERE (t0.UnitPrice__c > {0})", value);
            string actual = linq.ToString();

            Assert.That(actual, EqualTo(expected));


            var res = linq.ToList();

            Assert.That(res, Is.All.GreaterThan(value));
        }


        [Test]
        public void NullParameter()
        {
            var linq =
                from p in SforceManager.Context.GetTable<Supplier__c>()
                where p.Fax__c != null
                select p.Fax__c;

            string expected = "SELECT t0.Fax__c FROM Supplier__c AS t0 WHERE (t0.Fax__c != null)";
            string actual = linq.ToString();

            Assert.That(actual, EqualTo(expected));
            
            var res = linq.ToList();

            Assert.That(res, Is.All.Not.Null);
        }

        [Test]        
        public void BoolParameter([Values(true, false)]bool value)
        {
            var linq =
                from p in SforceManager.Context.GetTable<Product__c>()
                where p.Discontinued__c == value
                select new {p.Name, p.Discontinued__c};

            string expected = String.Format("SELECT t0.Name, t0.Discontinued__c FROM Product__c AS t0 WHERE (t0.Discontinued__c = {0})", value);
            string actual = linq.ToString();

            Assert.That(actual, EqualTo(expected));
            
            var res = linq.ToList();

            Assert.That(res.Select(p => p.Discontinued__c), Is.All.EqualTo(value));
        }

        [Test]
        public void WithChildToParentRelationShip()
        {
            var linq =
                from p in SforceManager.Context.GetTable<Product__c>()
                where p.Category__r.Name.StartsWith("C")
                select new { Product = p.Name, Category = p.Category__r.Name };

            string expected = "SELECT t0.Name, t0.Category__r.Name FROM Product__c AS t0 WHERE t0.Category__r.Name LIKE 'C%' ";
            string actual = linq.ToString();

            Assert.That(expected, EqualTo(actual));

            var result = linq.ToList();
            Assert.That(result.Select(p => p.Category), Is.All.StartsWith("C"));
        }

        [Test]
        public void WithParentToChildRelationShip()
        {
            var linq =
                from c in SforceManager.Context.GetTable<Category__c>()
                select new
                {
                    Category = c.Name,
                    Products = from p in c.Products__r
                               where p.UnitPrice__c < 10
                               select new {p.Name, p.UnitPrice__c}
                };

            string pattern = 
                "SELECT t0.Name, (" +
                    "SELECT t2.Name, t2.UnitPrice__c" +
                    "FROM t0.Products__r AS t2 " +
                    "WHERE (t2.UnitPrice__c < 10)" +
                ") " +
                "FROM Category__c AS t0";

            string query = linq.ToString();
            Assert.That(query, EqualTo(pattern));

            var result = linq.ToList();
            Assert.That(result.SelectMany(p => p.Products.Select(c => c.UnitPrice__c)), Is.All.LessThan(10));
        }
    }
}