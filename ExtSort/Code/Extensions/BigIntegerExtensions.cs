using System.Numerics;

namespace ExtSort.Code.Extensions
{
    internal static class BigIntegerExtensions
    {
        public static Span<char> AsSpan(this BigInteger number, Span<char> buffer)
        {
            int charsWritten;
            if (!number.TryFormat(buffer, out charsWritten))
                throw new InvalidOperationException("Cannot parse the source digit.");
            return buffer.Slice(0, charsWritten);
        }
        public static Span<char> AsSpan(this BigInteger number, int numOfDigits)
            => number.AsSpan(new char[numOfDigits]);
    }
}
