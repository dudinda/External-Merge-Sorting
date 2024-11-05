using TestTask.Code.Comparers;

namespace TestTask.Code.Extensions
{
    internal static class ArrayExtenisons
    {
        public static T[] SortWith<T>(this T[] source, params Comparison<T>[] comparisons)
        {
            if (comparisons.Any())
            {
                Array.Sort(source, 0, source.Length, new TaskTemplateComparer<T>(comparisons));
            }

            return source;
        }
    }
}
