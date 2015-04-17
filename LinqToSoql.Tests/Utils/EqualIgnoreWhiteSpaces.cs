using NUnit.Framework.Constraints;

namespace LinqToSoql.Tests.Utils
{
    public class EqualIgnoreWhiteSpaces : EqualConstraint
    {
        private string _expected;

        public EqualIgnoreWhiteSpaces(object expected)
            : base(expected)
        {
            _expected = ((string) expected).RemoveWhiteSpaces();
        }

        public override bool Matches(object actual)
        {
            string res = ((string) actual).RemoveWhiteSpaces();
            return res == _expected;
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.DisplayDifferences(_expected, actual);
        }

    }
}