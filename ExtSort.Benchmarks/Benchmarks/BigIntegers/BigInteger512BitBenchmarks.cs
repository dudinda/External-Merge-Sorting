using BenchmarkDotNet.Attributes;

namespace ExtSort.Benchmarks.Benchmarks.BigIntegers
{
    [SimpleJob(launchCount: 3, warmupCount: 10, invocationCount: 1000)]
    public class BigInteger512BitBenchmarks : BaseBigIntegerBenchmarks
    {
        public BigInteger512BitBenchmarks() : base(512) { }
    }
}
