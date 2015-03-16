using System;
using System.Linq;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace LinqToSoql.Tests
{
    [TestFixture]
    public class WhereTests
    {
        private static SforceContext _context;


        [SetUp]
        public void Init()
        {
            //TODO use fake context
            if (_context == null)
                _context = new SforceContext(Constants.Username, Constants.Password, Constants.Token);
        }

        [Test]
        public void Simple()
        {
            var q =
                from p in _context.GetTable<Product__c>()
                where p.UnitPrice__c > 40
                select p.UnitPrice__c;

            string pattern = "SELECT t0.UnitPrice__c FROM Product__c AS t0 WHERE (t0.UnitPrice__c > 40)";

            string soql = q.ToString().Replace("\n", " ").Replace("\r", String.Empty);
            Assert.That(soql, Is.EqualTo(pattern));
             

            var res = q.ToList();

            Assert.That(res, Is.Not.Empty);
            Assert.That(res, Is.All.GreaterThan(40));
        }

        [Test]
        public void IntParameter()
        {
            int value = 40;
            var q =
                from p in _context.GetTable<Product__c>()
                where p.UnitPrice__c > value
                select p.UnitPrice__c;

            string pattern = String.Format("SELECT t0.UnitPrice__c FROM Product__c AS t0 WHERE (t0.UnitPrice__c > {0})", value);

            string soql = q.ToString().Replace("\n", " ").Replace("\r", String.Empty);
            Assert.That(soql, Is.EqualTo(pattern));


            var res = q.ToList();

            Assert.That(res, Is.Not.Empty);
            Assert.That(res, Is.All.GreaterThan(value));
        }


        [Test]
        public void NullParameter()
        {
            var q =
                from p in _context.GetTable<Supplier__c>()
                where p.Fax__c != null
                select p.Fax__c;

            string pattern = "SELECT t0.Fax__c FROM Supplier__c AS t0 WHERE (t0.Fax__c != null)";

            string soql = q.ToString().Replace("\n", " ").Replace("\r", String.Empty);
            Assert.That(soql, Is.EqualTo(pattern));


            var res = q.ToList();

            Assert.That(res, Is.Not.Empty);
            Assert.That(res, Is.All.Not.Null);
        }

        [Test]        
        public void BoolParameter([Values(true, false)]bool value)
        {
            var q =
                from p in _context.GetTable<Product__c>()
                where p.Discontinued__c == value
                select new {p.Name, p.Discontinued__c};

            string pattern = String.Format("SELECT t0.Name, t0.Discontinued__c FROM Product__c AS t0 WHERE (t0.Discontinued__c = {0})", value);

            string soql = q.ToString().Replace("\n", " ").Replace("\r", String.Empty);
            Assert.That(soql, Is.EqualTo(pattern));


            var res = q.ToList();

            Assert.That(res.Select(p => p.Discontinued__c), Is.All.EqualTo(value));
        }

    }
}