using System.Numerics;

namespace ExtSort.Code.Extensions
{
    internal static class StringExtensions
    {
        public static bool TryParsePriority(this string input, out (string Str, BigInteger Int) result)
        {
            result = default;
            if (string.IsNullOrEmpty(input))
                return false;

            var span = input.AsSpan();
            var idx = span.IndexOf('.');
            if (idx == -1)
                return false;

            if (BigInteger.TryParse(span.Slice(0, idx), out result.Int))
            {
                result.Str = span.Slice(idx + 1).ToString();
                return true;
            }

            return false;
        }

        public static string Eclipse(this string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length <= maxLength) return input;
            const string eclipse = "...";
            var firstPart = input.AsSpan(0, maxLength / 2);
            var lastPart = input.AsSpan(input.Length - maxLength / 2);
            return string.Concat(firstPart, eclipse, lastPart);
        }
    }
}
