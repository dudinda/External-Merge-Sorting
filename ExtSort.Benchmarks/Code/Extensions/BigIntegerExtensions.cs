using System.Numerics;

namespace ExtSort.Benchmarks.Code.Extensions
{
    internal static class BigIntegerExtensions
    {
        public static BigInteger NextBig(this Random rand, int bitLength)
        {
            if (bitLength < 1) return BigInteger.Zero;

            int bytes = bitLength / 8;
            int bits = bitLength % 8;

            byte[] bs = new byte[bytes + 1];
            rand.NextBytes(bs);

            byte mask = (byte)(0xFF >> (8 - bits));
            bs[bs.Length - 1] &= mask;

            return new BigInteger(bs);
        }
    }
}
