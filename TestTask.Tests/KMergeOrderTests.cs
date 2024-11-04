using TestTask.Code.Comparators;

namespace TestTask.Tests
{
    [TestClass]
    internal class KMergeOrderTests
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
    }
}
