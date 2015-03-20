using LinqToSoql.Sforce;

namespace LinqToSoql.Tests
{
    public class SforceManager
    {
        private static readonly SforceContext _context = new SforceContext(Constants.Username, Constants.Password, Constants.Token);

        public static SforceContext Context { get { return _context; } }
    }
}