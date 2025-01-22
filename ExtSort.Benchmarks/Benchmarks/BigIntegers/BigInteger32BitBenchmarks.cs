using BenchmarkDotNet.Attributes;

namespace ExtSort.Benchmarks.Benchmarks.BigIntegers
{
    [SimpleJob(launchCount: 3, warmupCount: 10, invocationCount: 1000)]
    public class BigInteger32BitBenchmarks : BaseBigIntegerBenchmarks
    {
        public BigInteger32BitBenchmarks() : base(32) { }
    }
}
