namespace TestTask.Code.Comparers
{
    internal class TaskTemplateComparer<T> : IComparer<T>
    {
        private readonly IEnumerable<Comparison<T>> _comparisons;
        private readonly CancellationToken _token;

        public TaskTemplateComparer(IEnumerable<Comparison<T>> comparisons, CancellationToken token = default)
        {
            _comparisons = comparisons;
            _token = token;
        }

        public int Compare(T x, T y)
        {
            using var iterator = _comparisons.GetEnumerator();
            while(iterator.MoveNext() && !_token.IsCancellationRequested)
            {
                var result = iterator.Current(x, y);
                if (result != 0)
                    return result;
            }
            _token.ThrowIfCancellationRequested();
            return 0;
        }
    }
}
