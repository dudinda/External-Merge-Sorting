namespace TestTask.Code.Comparers
{
    internal class TaskTemplateComparer<T> : IComparer<T>
    {
        private readonly IEnumerable<Comparison<T>> _comparisons;

        public TaskTemplateComparer(IEnumerable<Comparison<T>> comparisons)
        {
            _comparisons = comparisons;
        }

        public int Compare(T x, T y)
        {
            foreach (var comparison in _comparisons)
            {
                var result = comparison(x, y);
                if (result != 0)
                    return result;
            }
            return 0;
        }
    }
}
