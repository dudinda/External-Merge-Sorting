using System.Text;

using TestTask.Code.Comparers;
using TestTask.Code.Extensions;

namespace TestTask.Tests
{
    [TestClass]
    public class SortOrderTests
    {
        private TaskTemplateComparer<(string Str, int Int)> _comparer;
        private string[] _data;
        private string[] _correctData;

        [TestInitialize]
        public void Setup()
        {
            var comparisons = new Comparison<(string Str, int Int)>[]
            {
                (x, y) => x.Str.CompareTo(y.Str),
                (x, y) => x.Int.CompareTo(y.Int)
            };
            _comparer = new TaskTemplateComparer<(string Str, int Int)>(comparisons);
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
            var buffer = new (string Str, int Int)[_data.Length];
            for(var i = 0; i < _data.Length; ++i)
            {
                if (_data[i].TryParseLine(out var entry))
                {
                    buffer[i] = entry.Priority;
                }
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