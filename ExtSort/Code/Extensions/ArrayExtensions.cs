using ExtSort.Code.Comparers;

namespace ExtSort.Code.Extensions
{
    internal static class ArrayExtensions
    {
        public static T[] SortWith<T>(this T[] source, CancellationToken token, params Comparison<T>[] comparisons)
        {
            if (comparisons.Any())
            {
                using var comparer = new MultiColumnComparer<T>(comparisons, token);
                Array.Sort(source, 0, source.Length, comparer);
            }

            return source;
        }
    }
}
