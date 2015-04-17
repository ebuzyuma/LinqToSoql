using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace LinqToSoql.Tests.Utils
{
    public class TestsBase
    {
        protected Func<string, Constraint> EqualTo;

        [SetUp]
        public void Init()
        {
            EqualTo = s => new EqualIgnoreWhiteSpaces(s);
        }

         
    }
}