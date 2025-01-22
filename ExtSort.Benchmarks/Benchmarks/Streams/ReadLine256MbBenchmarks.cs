using BenchmarkDotNet.Attributes;

namespace ExtSort.Benchmarks.Benchmarks.Streams
{
    [SimpleJob(launchCount: 3, warmupCount: 5, invocationCount: 15)]
    public class ReadLine256MbBenchmarks : BaseReadLineBenchmarks 
    {
        public ReadLine256MbBenchmarks() : base(256) { }
    }
}
