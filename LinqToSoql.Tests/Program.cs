using System;
using System.Linq;
using LinqToSoql.Sforce;
using LinqToSoql.Tests.Models;

namespace LinqToSoql.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string username = "<your_username>";
            string password = "<password>";
            string token = "<token>";

            var context = new SforceContext(username, password, token);

            var q =
                from p in context.GetTable<Account>()
                select new {p.Name, p.Phone};
                
            Console.WriteLine(q);
            Console.WriteLine(String.Join("\n", q.ToList().Select(p => String.Format("{0,40} - {1}", p.Name, p.Phone))));
            Console.ReadLine();
        }
    }
}
