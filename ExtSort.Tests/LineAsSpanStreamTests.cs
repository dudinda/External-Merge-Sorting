using ExtSort.Code.Streams;
using ExtSort.Models.Settings;
using ExtSort.Services.Generator;

using System.Text;

namespace ExtSort.Tests
{
    [TestClass]
    public class LineAsSpanStreamTests
    {
        private readonly string _filename = "filename.txt";
        private readonly GeneratorService _service = new GeneratorService(new GeneratorSettings());
        private readonly List<string> _buffer = new();

        [TestInitialize]
        public void Setup()
        {
            _service.Generate(_filename, 1024 * 8, CancellationToken.None).Wait();
            using var file = File.OpenRead(_filename);
            using var reader = new StreamReader(file, new UTF8Encoding(false), false, 4096);
            string line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                _buffer.Add(line);
            }
        }

        [TestMethod]
        public void VerifyCorrectOrderWithLineAsSpan()
        {
            var target = new List<string>();
            using var file = File.OpenRead(_filename);
            using var reader = new LineAsSpanReader(file, new UTF8Encoding(false), false, 4096);
            Memory<char> line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLineAsMemory();
                target.Add(line.ToString());
            }

            CollectionAssert.AreEqual(_buffer, target);
        }
    }
}
