using BenchmarkDotNet.Attributes;

namespace ExtSort.Benchmarks.Benchmarks.Streams
{
    [SimpleJob(launchCount: 3, warmupCount: 5, invocationCount: 15)]
    public class ReadLine1024MbBenchmarks : BaseReadLineBenchmarks
    {
        public ReadLine1024MbBenchmarks() : base(1024) { }
    }
}
