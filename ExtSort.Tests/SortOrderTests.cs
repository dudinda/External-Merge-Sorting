using ExtSort.Code.Comparers;
using ExtSort.Code.Extensions;
using System.Numerics;
using System.Text;

namespace ExtSort.Tests
{
    [TestClass]
    public class SortOrderTests
    {
        private MultiColumnComparer<(string Str, BigInteger Int)> _comparer;
        private string[] _data;
        private string[] _correctData;

        [TestInitialize]
        public void Setup()
        {
            var comparisons = new Comparison<(string Str, BigInteger Int)>[]
            {
                (x, y) => x.Str.AsSpan().CompareTo(y.Str.AsSpan(), StringComparison.Ordinal),
                (x, y) => x.Int.CompareTo(y.Int)
            };
            _comparer = new MultiColumnComparer<(string Str, BigInteger Int)>(comparisons);
            _data =
            [
                "415. Apple",
                "30432. Something something something",
                "1. Apple",
                "32. Cherry is the best",
                "2. Banana is yellow"
            ];
            _correctData =
            [
                "1. Apple",
                "415. Apple",
                "2. Banana is yellow",
                "32. Cherry is the best",
                "30432. Something something something"
            ];
        }

        [TestMethod]
        public void VerifyCorrectOrderWithTaskTestComparatorSort()
        {
            var buffer = new (string Str, BigInteger Int)[_data.Length];
            for(var i = 0; i < _data.Length; ++i)
            {
                if (_data[i].TryParsePriority(out var priority))
                    buffer[i] = priority;
            }

            Array.Sort(buffer, 0, buffer.Length, _comparer);
            var result = new string[buffer.Length];
            var builder = new StringBuilder();
            for (var i = 0; i < _data.Length; ++i)
            {
                builder.Append(buffer[i].Int).Append(".").Append(buffer[i].Str);
                result[i] = builder.ToString();
                builder.Clear();
            }
            CollectionAssert.AreEqual(_correctData, result);
        }
    }
}