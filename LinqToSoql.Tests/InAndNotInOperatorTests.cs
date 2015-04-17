using System;
using System.Collections.Generic;
using System.Linq;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;
using NUnit.Framework;

namespace LinqToSoql.Tests
{
    [TestFixture]
    public class InAndNotInOperatorTests
    {
        [Test]
        public void SimpleIn()
        {
            var categories = new List<string> { "Condiments", "Confections" };
            var q =
                from p in SforceManager.Context.GetTable<Product__c>()
                where categories.Contains(p.Category__r.Name)
                select new {p.UnitPrice__c, p.Category__r.Name};

            string pattern = "SELECT t0.UnitPrice__c, t0.Category__r.Name FROM Product__c AS t0 " +
                             "WHERE t0.Category__r.Name IN ('Condiments', 'Confections')";

            string soql = q.ToString();
            Assert.That(soql.IsEqualIgnoreWhiteSpaces(pattern));


            var res = q.ToList();

            Assert.That(res.Select(p => p.Name).Distinct(), Is.EquivalentTo(categories));
        }

        [Test]
        public void SimpleNotIn()
        {
            var categories = new List<string> { "Condiments", "Confections" };
            var q =
                from p in SforceManager.Context.GetTable<Product__c>()
                where !categories.Contains(p.Category__r.Name)
                select new { p.UnitPrice__c, p.Category__r.Name };

            string pattern = "SELECT t0.UnitPrice__c, t0.Category__r.Name FROM Product__c AS t0 " +
                             "WHERE t0.Category__r.Name NOT IN ('Condiments', 'Confections')";

            string soql = q.ToString();
            Assert.That(soql.IsEqualIgnoreWhiteSpaces(pattern));


            var res = q.ToList();

            Assert.That(res.Select(p => p.Name).Distinct().Intersect(categories), Is.Empty);
        }

    }
}