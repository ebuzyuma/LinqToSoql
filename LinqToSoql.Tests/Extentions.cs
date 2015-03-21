using System;
using System.Text.RegularExpressions;
using NUnit.Framework.Constraints;

namespace LinqToSoql.Tests
{
    public static class Extentions
    {
        public static string RemoveWhiteSpaces(this string input)
        {
            return Regex.Replace(input, @"[\n\r ]", String.Empty);
        }

        public static bool IsEqualIgnoreWhiteSpaces(this string input, string expected)
        {
            return input.RemoveWhiteSpaces() == expected.RemoveWhiteSpaces();
        }
    }
}