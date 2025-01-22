using BenchmarkDotNet.Attributes;

namespace ExtSort.Benchmarks.Benchmarks.Streams
{
    [SimpleJob(launchCount: 3, warmupCount: 5, invocationCount: 15)]
    public class ReadLine64MbBenchmarks : BaseReadLineBenchmarks
    {
        public ReadLine64MbBenchmarks() : base(64) { }
    }
}
