using System.Linq;

namespace Xtricate.DocSet
{
    public static class StringExtensions
    {
        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());
        }
    }
}