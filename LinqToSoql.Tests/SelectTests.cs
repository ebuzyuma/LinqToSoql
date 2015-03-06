using System.Linq;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;
using NUnit.Framework;

namespace LinqToSoql.Tests.Properties
{

    [TestFixture]
    public class SelectTests
    {
        private const string Username = "ebuzyuma@linq.com";
        private const string Password = "7229124,tf";
        private const string Token = "rTC3Txi0gY4uGyxlJ8YyEIW5";

        private SforceContext _context;

        
        [SetUp]
        public void Init()
        {
            //TODO use fake context
            _context = new SforceContext(Username, Password, Token);
        }
        [Test]
        public void SelectSimple()
        {
            var q =
                from p in _context.GetTable<Account>()
                select new {p.Name, p.Phone};

            string soql = q.ToString();
            Assert.That(soql, Is.Not.Empty);
            StringAssert.Contains("SELECT",soql);
            StringAssert.Contains("Name", soql);
            StringAssert.Contains("Phone", soql);
            StringAssert.Contains("Account", soql);

            var res = q.ToList();

            Assert.That(res, Is.Not.Empty);
        }

        [Test]
        public void Where()
        {
            var name = "sForce";
            var q =
                from p in _context.GetTable<Account>()
                where p.Name == name
                select new {p.Name, p.Phone};

            string soql = q.ToString();
            string lang = "[a-zA-Z0-9 .,']+";
            string where = lang + "WHERE" + lang + "Name = '" + name + lang;
            StringAssert.IsMatch(where, soql);
            StringAssert.Contains("sForce", soql);

            var res = q.ToList();

            Assert.That(res, Is.Not.Empty);
            Assert.That(res.Select(p => p.Name), Is.All.EqualTo(name));
        }
    }
}