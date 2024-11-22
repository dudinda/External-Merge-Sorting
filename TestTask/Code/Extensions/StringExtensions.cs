using TestTask.Models.Sorter;

namespace TestTask.Code.Extensions
{
    internal static class StringExtensions
    {
        public static bool TryParsePriority(this string input, out (string Str, int Int) result)
        {
            result = default;
            if (string.IsNullOrEmpty(input))
                return false;

            var parts = input.Split('.');
            if (parts.Length > 1)
            {
                if (int.TryParse(parts[0], out var integer))
                {
                    result = (parts[1], integer);
                    return true;
                }
            }

            return false;
        }
    }
}
