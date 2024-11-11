using TestTask.Code.Comparers;

namespace TestTask.Code.Extensions
{
    internal static class ArrayExtenisons
    {
        public static T[] SortWith<T>(this T[] source, CancellationToken token, params Comparison<T>[] comparisons)
        {
            if (comparisons.Any())
            {
                using var comparer = new TaskTemplateComparer<T>(comparisons, token);
                Array.Sort(source, 0, source.Length, comparer);
            }

            return source;
        }
    }
}
