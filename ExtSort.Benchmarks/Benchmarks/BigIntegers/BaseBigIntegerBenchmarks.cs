using BenchmarkDotNet.Attributes;

using ExtSort.Benchmarks.Code.Extensions;
using ExtSort.Code.Extensions;

using System.Buffers;
using System.Numerics;

namespace ExtSort.Benchmarks.Benchmarks.BigIntegers
{
    public abstract class BaseBigIntegerBenchmarks
    {
        private readonly int _length;
        private readonly BigInteger _target;
        private readonly Random _generator = new Random();
        private readonly ArrayPool<char> _shared = ArrayPool<char>.Shared;

        public BaseBigIntegerBenchmarks(int bitLength)
        {
            _target = _generator.NextBig(bitLength);
            _length = _target.ToString().Length;
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

        [Benchmark]
        public void BigIntegerAsSpanStackAllow()
        {
            _target.AsSpan(stackalloc char[_length]);
        }
    }
}
