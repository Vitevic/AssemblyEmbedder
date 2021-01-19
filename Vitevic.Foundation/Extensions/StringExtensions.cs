using System;

namespace Vitevic.Foundation.Extensions
{
    public static class StringExtensions
    {
        /// <returns>True is the two strings are equals.</returns>
        public static bool IsEqualNoCase(this string a, string b)
        {
            return 0 == string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsNoCase(this string a, string b)
        {
            return a.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string Arguments(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
