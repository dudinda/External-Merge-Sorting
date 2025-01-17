using ExtSort.Code.Comparers;
using ExtSort.Code.Extensions;
using ExtSort.Models.Sorter;

using System;
using System.Numerics;

namespace ExtSort.Tests
{
    [TestClass]
    public class KMergeOrderTests
    {
        private MultiColumnComparer<(ReadOnlyMemory<char> Str, BigInteger Int)> _comparer;
        private Queue<Memory<char>> _1stList;
        private Queue<Memory<char>> _2ndList;
        private Queue<Memory<char>> _3rdList;
        private List<Memory<char>> _result;

        [TestInitialize]
        public void Setup()
        {
            var comparisons = new Comparison<(ReadOnlyMemory<char> Str, BigInteger Int)>[]
            {
                (x, y) => x.Str.Span.CompareTo(y.Str.Span, StringComparison.Ordinal),
                (x, y) => x.Int.CompareTo(y.Int)
            };
            _comparer = new MultiColumnComparer<(ReadOnlyMemory<char> Str, BigInteger Int)>(comparisons);
            _1stList = new Queue<Memory<char>>(Get1stOrderedList().Select(_ => new Memory<char>(_.ToArray())));
            _2ndList = new Queue<Memory<char>>(Get2ndOrderedList().Select(_ => new Memory<char>(_.ToArray())));
            _3rdList = new Queue<Memory<char>>(Get3rdOrderedList().Select(_ => new Memory<char>(_.ToArray())));
            _result = new List<Memory<char>>(_1stList.Count + _2ndList.Count + _3rdList.Count);
        }

        [TestMethod]
        public void VerifyCorrectOrderWithTaskTestComparatorMerge()
        {
            var array = new Queue<Memory<char>>[] { _1stList, _2ndList, _3rdList };
            var queue = new PriorityQueue<Entry, (ReadOnlyMemory<char>, BigInteger)>(array.Length, _comparer);
            for (var i = 0; i < array.Length; ++i)
            {
                var value = array[i].Dequeue();
                if (value.TryParsePriority(out var priority))
                {
                    Entry row = new(value, i);
                    queue.Enqueue(row, priority);
                }
            }
            var finishedLists = new HashSet<int>();
            while (finishedLists.Count != array.Length)
            {
                var entry = queue.Dequeue();
                var streamReaderIndex = entry.Index;
                _result.Add(entry.Row);

                if (array[streamReaderIndex].TryDequeue(out var value) && value.TryParsePriority(out var priority))
                {
                    Entry row = new(value, streamReaderIndex);
                    queue.Enqueue(row, priority);
                    continue;
                }

                if (array[streamReaderIndex].Count == 0)
                    finishedLists.Add(streamReaderIndex);
            }

            CollectionAssert.AreEqual(_result.Select(_ => _.ToString()).ToArray(), CorrectMergeOrder().ToArray());
        }

        private IEnumerable<string> Get1stOrderedList()
        {
            yield return "1. Apple";
            yield return "415. Apple";
            yield return "2. Banana is yellow";
            yield return "32. Cherry is the best";
            yield return "30432. Something something something";
        }

        private IEnumerable<string> Get2ndOrderedList()
        {
            yield return "10. Apple";
            yield return "21. Banana is yellow";
            yield return "40. Cherry is the best";
            yield return "30432. Something something something";
        }

        private IEnumerable<string> Get3rdOrderedList()
        {
            yield return "3. Apple";
            yield return "11. Apple";
            yield return "0. Banana is yellow";
            yield return "1. Something something something";
        }

        private IEnumerable<string> CorrectMergeOrder()
        {
            yield return "1. Apple";
            yield return "3. Apple";
            yield return "10. Apple";
            yield return "11. Apple";
            yield return "415. Apple";
            yield return "0. Banana is yellow";
            yield return "2. Banana is yellow";
            yield return "21. Banana is yellow";
            yield return "32. Cherry is the best";
            yield return "40. Cherry is the best";
            yield return "1. Something something something";
            yield return "30432. Something something something";
            yield return "30432. Something something something";
        }
    }
}
