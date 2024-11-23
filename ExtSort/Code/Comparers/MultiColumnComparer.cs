namespace ExtSort.Code.Comparers
{
    internal class MultiColumnComparer<T> : IComparer<T>, IDisposable
    {
        private readonly IEnumerator<Comparison<T>> _iterator;
        private readonly CancellationToken _token;

        public MultiColumnComparer(IEnumerable<Comparison<T>> comparisons, CancellationToken token = default)
        {
            _iterator = comparisons.GetEnumerator();
            _token = token;
        }

        public int Compare(T x, T y)
        {
            _iterator.Reset();
            while(_iterator.MoveNext() && !_token.IsCancellationRequested)
            {
                var result = _iterator.Current(x, y);
                if (result != 0)
                    return result;
            }
            _token.ThrowIfCancellationRequested();
            return 0;
        }

        public void Dispose()
        {
            _iterator.Dispose();
        }
    }
}
