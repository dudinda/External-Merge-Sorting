using BenchmarkDotNet.Attributes;

using ExtSort.Code.Extensions;

using System.Buffers;
using System.Numerics;

namespace ExtSort.Benchmarks.BigInt
{
    [SimpleJob(launchCount: 3, warmupCount: 10, invocationCount: 100)]
    public class BigIntegerLargeBenchmarks
    {
        private BigInteger _target;
        private int _length;
        private ArrayPool<char> _shared;

        [GlobalSetup]
        public void Setup()
        {
            const string NUMBER = "50000000000000000000000000000000000000000000";
            _target = BigInteger.Parse(NUMBER);
            _length = NUMBER.Length;
            _shared = ArrayPool<char>.Shared;
        }

        [Benchmark]
        public void BigIntegerToString()
        {
            _target.ToString();
        }

        [Benchmark]
        public void BigIntegerAsSpan()
        {
            _target.AsSpan(_length);
        }

        [Benchmark]
        public void BigIntegerAsSpanWithPool()
        {
            var rented = _shared.Rent(_length);
            _target.AsSpan(rented);
            _shared.Return(rented);
        }
    }
}
