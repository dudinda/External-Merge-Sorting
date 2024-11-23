namespace ExtSort.Code.Extensions
{
    internal static class StringExtensions
    {
        public static bool TryParsePriority(this string input, out (string Str, int Int) result)
        {
            result = default;
            if (string.IsNullOrEmpty(input))
                return false;

            var span = input.AsSpan();
            var idx = span.IndexOf('.');
            if (idx == -1)
                return false;

            if (int.TryParse(span.Slice(0, idx), out result.Int))
            {
                result.Str = span.Slice(idx + 1).ToString();
                return true;
            }

            return false;
        }
    }
}
