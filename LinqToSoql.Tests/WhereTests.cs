using System;
using System.Linq;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;
using NUnit.Framework;

namespace LinqToSoql.Tests
{
    [TestFixture]
    public class WhereTests
    {
        private SforceContext _context;


        [SetUp]
        public void Init()
        {
            //TODO use fake context
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
    }
}