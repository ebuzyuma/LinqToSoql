using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using LinqToSoql.PartnerSforce;

namespace LinqToSoql.PartnerSforce 
{
    public partial class sObject
    {
        public object GetValue(string name)
        {
            return Any.First(p => p.Name == "sf:" + name).InnerText;
        }

        public sObject GetProperty(string name)
        {
            XmlElement[] elems = Any.First(p => p.Name == "sf:" + name).ChildNodes.Cast<XmlElement>().ToArray();
            return new sObject {Any = elems};
        }
    }
}

namespace LinqToSoql
{
    public static class Extentions
    {
        /// <summary>
        /// Performs a case-insensitive match
        /// </summary>
        /// <param name="soqlPattern">string pattern with wildcard.
        /// The % wildcard matches zero or more characters.
        /// The _ wildcard matches exactly one character.
        /// </param>
        /// <returns></returns>
        public static bool Like(this string input, string soqlPattern)
        {
            return Regex.IsMatch(input, Tool.ToCSharRegex(soqlPattern), RegexOptions.IgnoreCase);
        }
    }

    public class Tool
    {
        public static string ToCSharRegex(string soqlPattern)
        {
            const string exactlyOneWildcard = @"(?<!\\)_"; // for matching _ and not matching \_
            const string zeroOrMoreWildcard = @"(?<!\\)%"; // for matching % and not matching \%
            string pattern = Regex.Replace(soqlPattern, exactlyOneWildcard, ".");
            return Regex.Replace(pattern, zeroOrMoreWildcard, ".*");
        }
    }
}