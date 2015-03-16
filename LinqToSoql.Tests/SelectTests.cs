﻿using System;
using System.Linq;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;
using NUnit.Framework;

namespace LinqToSoql.Tests.Properties
{

    [TestFixture]
    public class SelectTests
    {
        private SforceContext _context;

        
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
            var linq = 
                from p in _context.GetTable<Category__c>()
                select p.Name;

            string pattern = "SELECT t0.Name FROM Category__c AS t0";

            string query = linq.ToString().Replace("\n", " ").Replace("\r", String.Empty);

            Assert.That(pattern, Is.EqualTo(query));
        }

        [Test]
        public void Projection()
        {
            var linq =
                from p in _context.GetTable<Category__c>()
                select new {p.Name, p.Description__c};

            var qwer =
                from p in _context.GetTable<Category__c>()
                select p;

            string pattern = "SELECT t0.Name, t0.Description__c FROM Category__c AS t0";

            string query = linq.ToString().Replace("\n", " ").Replace("\r", String.Empty);

            Assert.That(pattern, Is.EqualTo(query));


            var result = linq.ToList();

            Assert.That(result.Select(p => p.Name), Is.All.Not.Empty);
            Assert.That(result.Select(p => p.Description__c), Is.All.Not.Empty);
        }

        [Test]
        public void ChildToParentRelationShip()
        {
            var linq =
                from p in _context.GetTable<Product__c>()
                select new {Product = p.Name, Category = p.Category__r.Name};

            string pattern = "SELECT t0.Name, t0.Category__r.Name FROM Product__c AS t0";

            string query = linq.ToString().Replace("\n", " ").Replace("\r", String.Empty);

            Assert.That(pattern, Is.EqualTo(query));

            var result = linq.ToList();
            Assert.That(result.Select(p => p.Product), Is.All.Not.Empty);
            Assert.That(result.Select(p => p.Category), Is.All.Not.Empty);
        }

        [Test]
        public void ParentToChildRelationShip()
        {
            //TODO fix
            var linq =
                from p in _context.GetTable<Category__c>()
                select new
                       {
                           Category = p.Name,
                           Products = from c in p.Products__r
                                      select c.Name
                       };

            string pattern = "SELECT t0.Name, (SELECT t1.Name FROM t0.Products__r AS t1) FROM Category__c AS t0";

            string query = linq.ToString().Replace("\n", " ").Replace("\r", String.Empty);

            Assert.That(pattern, Is.EqualTo(query));

            var result = linq.ToList();
            Assert.That(result.Select(p => p.Products), Is.All.Not.Empty);
            Assert.That(result.Select(p => p.Category), Is.All.Not.Empty);
        }
    }
}