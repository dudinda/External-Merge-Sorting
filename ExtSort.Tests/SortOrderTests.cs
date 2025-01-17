using ExtSort.Code.Comparers;
using ExtSort.Code.Extensions;

using System;
using System.Numerics;
using System.Text;

namespace ExtSort.Tests
{
    [TestClass]
    public class SortOrderTests
    {
        private MultiColumnComparer<(ReadOnlyMemory<char> Str, BigInteger Int)> _comparer;
        private Memory<char>[] _data;
        private Memory<char>[] _correctData;

        [TestInitialize]
        public void Setup()
        {
            var comparisons = new Comparison<(ReadOnlyMemory<char> Str, BigInteger Int)>[]
            {
                (x, y) => x.Str.Span.CompareTo(y.Str.Span, StringComparison.Ordinal),
                (x, y) => x.Int.CompareTo(y.Int)
            };
            _comparer = new MultiColumnComparer<(ReadOnlyMemory<char> Str, BigInteger Int)>(comparisons);
            _data = GetData().Select(_ => new Memory<char>(_.ToArray())).ToArray();
            _correctData = GetCorrectData().Select(_ => new Memory<char>(_.ToArray())).ToArray();

        }

        [TestMethod]
        public void VerifyCorrectOrderWithTaskTestComparatorSort()
        {
            var buffer = new (ReadOnlyMemory<char> Str, BigInteger Int)[_data.Length];
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

            CollectionAssert.AreEqual(_correctData.Select(_ => _.ToString()).ToArray(), result);
        }

        private IEnumerable<string> GetData()
        {
            yield return "415. Apple";
            yield return "30432. Something something something";
            yield return "1. Apple";
            yield return "32. Cherry is the best";
            yield return "2. Banana is yellow";
        }

        private IEnumerable<string> GetCorrectData()
        {
            yield return "1. Apple";
            yield return "415. Apple";
            yield return "2. Banana is yellow";
            yield return "32. Cherry is the best";
            yield return "30432. Something something something";
        }
    }
}