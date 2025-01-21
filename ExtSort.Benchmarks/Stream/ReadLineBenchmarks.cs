using BenchmarkDotNet.Attributes;

using ExtSort.Code.Streams;
using ExtSort.Models.Settings;
using ExtSort.Services.Generator;

using System.Text;

namespace ExtSort.Benchmarks.Stream
{
    [SimpleJob(launchCount: 3, warmupCount: 5, invocationCount: 15)]
    public class ReadLineBenchmarks
    {
        private string _filename;

        [GlobalSetup]
        public void Setup()
        {
            _filename = "benchmark.txt";
            var service = new GeneratorService(new GeneratorSettings());
            service.Generate(_filename, 1024 * 40, CancellationToken.None).Wait();
        }

        [Benchmark]
        public void ReadLinesStreamReader()
        {
            using var file = File.OpenRead(_filename);
            using var reader = new StreamReader(file, new UTF8Encoding(false), false, 4096);
            string line;
            while((line = reader.ReadLine()) != null ) { }
        }

        [Benchmark]
        public void ReadLinesLinesAsSpan()
        {
            using var file = File.OpenRead(_filename);
            using var reader = new LineAsSpanReader(file, new UTF8Encoding(false), false, 4096);
            string line;
            while ((line = reader.ReadLine()) != null) { }
        }

        [GlobalCleanup]
        public void Clean()
        {
            File.Delete(_filename);
        }
    }
}
