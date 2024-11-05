﻿using TestTask.Code.Comparators;
using TestTask.Code.Extensions;
using TestTask.Models.Sorter;

namespace TestTask.Tests
{
    [TestClass]
    public class KMergeOrderTests
    {
        private TaskTemplateComparer<(string Str, int Int)> _comparer;
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
            _comparer = new TaskTemplateComparer<(string Str, int Int)>(comparisons);
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
                if (value.TryParseLine(out var entry))
                {
                    entry.Item.StreamReaderIdx = i;
                    queue.Enqueue(entry.Item, entry.Priority);
                }
            }
            var finishedLists = new HashSet<int>();
            while (finishedLists.Count != array.Length)
            {
                var entry = queue.Dequeue();
                var streamReaderIndex = entry.StreamReaderIdx;
                _result.Add(entry.Row);

                if (array[streamReaderIndex].TryDequeue(out var value) && value.TryParseLine(out var entryWithPriority))
                {
                    entryWithPriority.Item.StreamReaderIdx = streamReaderIndex;
                    queue.Enqueue(entryWithPriority.Item, entryWithPriority.Priority);
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