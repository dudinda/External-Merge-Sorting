using BenchmarkDotNet.Attributes;

using ExtSort.Code.Extensions;
using ExtSort.Code.Streams;
using ExtSort.Models.Settings;
using ExtSort.Services.Generator;

using System.Text;

namespace ExtSort.Benchmarks.Benchmarks.Streams
{
    public abstract class BaseReadLineBenchmarks
    {
        private readonly string _filename = "filename.txt";
        private readonly GeneratorService _service = new GeneratorService(new GeneratorSettings());

        public BaseReadLineBenchmarks(int fileSizeMb)
        {
            _service.Generate(_filename, 1024 * fileSizeMb, CancellationToken.None).Wait();
        }

        [Benchmark]
        public void ReadLinesAsString()
        {
            using var file = File.OpenRead(_filename);
            using var reader = new StreamReader(file, new UTF8Encoding(false), false, 4096);
            string line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
            }
        }

        [Benchmark]
        public void ReadLinesAsMemory()
        {
            using var file = File.OpenRead(_filename);
            using var reader = new LineAsSpanReader(file, new UTF8Encoding(false), false, 4096);
            Memory<char> line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLineAsMemory();
            }
        }

        [Benchmark]
        public void ReadLinesAsSpan()
        {
            using var file = File.OpenRead(_filename);
            using var reader = new LineAsSpanReader(file, new UTF8Encoding(false), false, 4096);
            Span<char> line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLineAsSpan();
            }
        }

        [Benchmark]
        public void ReadLinesAsStringThenParse()
        {
            using var file = File.OpenRead(_filename);
            using var reader = new StreamReader(file, new UTF8Encoding(false), false, 4096);
            string line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                line.TryParsePriority(out var priority);
            }
        }

        [Benchmark]
        public void ReadLinesAsMemoryThenParse()
        {
            using var file = File.OpenRead(_filename);
            using var reader = new LineAsSpanReader(file, new UTF8Encoding(false), false, 4096);
            Memory<char> line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLineAsMemory();
                line.TryParsePriority(out var priority);
            }
        }
    }
}
