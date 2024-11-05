using TestTask.Models.Sorter;

namespace TestTask.Code.Extensions
{
    internal static class StringExtensions
    {
        public static bool TryParseLine(this string input, out EntryWithPriority result)
        {
            result = null;
            if (string.IsNullOrEmpty(input))
                return false;

            if (!input.Contains("."))
                return false;

            var parts = input.Split(".");
            if (int.TryParse(parts[0], out var integer))
            {
                result = new EntryWithPriority
                {
                    Item = new Entry()
                    {
                        Row = input
                    },
                    Priority = (parts[1], integer)
                };
                return true;
            }

            return false;
        }
    }
}
