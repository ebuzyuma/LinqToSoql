using System;
using System.Linq;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;
using LinqToSoql.Tests.Utils;
using NUnit.Framework;

namespace LinqToSoql.Tests
{
    [TestFixture]
    public class LikeOperatorTests : TestsBase
    {
        [Test]
        public void StringExtention([Values("C%", "___food", "_e%")]string likePattern)
        {
            var linq =
                from p in SforceManager.Context.GetTable<Category__c>()
                where p.Name.Like(likePattern)
                select p.Name;

            string pattern = String.Format("SELECT t0.Name FROM Category__c AS t0 WHERE t0.Name LIKE '{0}'", likePattern);

            string query = linq.ToString();
            Assert.That(pattern, EqualTo(query));

            var res = linq.ToList();
            Assert.That(res, Is.All.Matches(Tool.ToCSharRegex(likePattern)));
        }

        [Test]
        public void StringStartsWith()
        {
            var linq =
                from p in SforceManager.Context.GetTable<Category__c>()
                where p.Name.StartsWith("C")
                select p.Name;

            string likePattern = "C%";
            string pattern = String.Format("SELECT t0.Name FROM Category__c AS t0 WHERE t0.Name LIKE '{0}'", likePattern);

            string query = linq.ToString();
            Assert.That(pattern, EqualTo(query));

            var res = linq.ToList();
            Assert.That(res, Is.All.Matches(Tool.ToCSharRegex(likePattern)));
        }

        [Test]
        public void StringEndsWith()
        {
            var linq =
                from p in SforceManager.Context.GetTable<Category__c>()
                where p.Name.EndsWith("ts")
                select p.Name;

            string likePattern = "%ts";
            string pattern = String.Format("SELECT t0.Name FROM Category__c AS t0 WHERE t0.Name LIKE '{0}'", likePattern);

            string query = linq.ToString();
            Assert.That(pattern, EqualTo(query));

            var res = linq.ToList();            
            Assert.That(res, Is.All.Matches(Tool.ToCSharRegex(likePattern)));
        }

        [Test]
        public void StringContains()
        {
            var linq =
                from p in SforceManager.Context.GetTable<Category__c>()
                where p.Name.Contains("ea")
                select p.Name;

            string likePattern = "%ea%";
            string pattern = String.Format("SELECT t0.Name FROM Category__c AS t0 WHERE t0.Name LIKE '{0}'", likePattern);

            string query = linq.ToString();
            Assert.That(pattern, EqualTo(query));

            var res = linq.ToList();
            Assert.That(res, Is.All.Matches(Tool.ToCSharRegex(likePattern)));
        }
    }
}