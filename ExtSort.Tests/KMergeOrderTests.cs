using ExtSort.Code.Comparers;
using ExtSort.Code.Extensions;
using ExtSort.Models.Sorter;

namespace ExtSort.Tests
{
    [TestClass]
    public class KMergeOrderTests
    {
        private MultiColumnComparer<(string Str, int Int)> _comparer;
        private Queue<string> _1stList;
        private Queue<string> _2ndList;
        private Queue<string> _3rdList;
        private List<string> _result;

        [TestInitialize]
        public void Setup()
        {
            var comparisons = new Comparison<(string Str, int Int)>[]
            {
                (x, y) => x.Str.CompareTo(y.Str),
                (x, y) => x.Int.CompareTo(y.Int)
            };
            _comparer = new MultiColumnComparer<(string Str, int Int)>(comparisons);
            _1stList = new Queue<string>(Get1stOrderedList());
            _2ndList = new Queue<string>(Get2ndOrderedList());
            _3rdList = new Queue<string>(Get3rdOrderedList());
            _result = new List<string>(_1stList.Count + _2ndList.Count + _3rdList.Count);
        }

        [TestMethod]
        public void VerifyCorrectOrderWithTaskTestComparatorMerge()
        {
            var array = new Queue<string>[] { _1stList, _2ndList, _3rdList };
            var queue = new PriorityQueue<Entry, (string, int)>(array.Length, _comparer);
            for (var i = 0; i < array.Length; ++i)
            {
                var value = array[i].Dequeue();
                if (value.TryParsePriority(out var priority))
                {
                    var row = new Entry() { Row = value, Index = i };
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
                    var row = new Entry() { Row = value, Index = streamReaderIndex };
                    queue.Enqueue(row, priority);
                    continue;
                }

                if (array[streamReaderIndex].Count == 0)
                {
                    finishedLists.Add(streamReaderIndex);
                }
            }

            CollectionAssert.AreEqual(_result, CorrectMergeOrder().ToArray());
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
