using System.Numerics;

namespace ExtSort.Code.Extensions
{
    internal static class BigIntegerExtensions
    {
        public static Span<char> AsSpan(this BigInteger number, int numOfDigits)
        {
            Span<char> bigValue = new char[numOfDigits]; 
            int charsWritten;
            if (!number.TryFormat(bigValue, out charsWritten))
                throw new InvalidOperationException("Cannot parse the source digit.");
            return bigValue.Slice(0, charsWritten);
        }

        public static Span<char> AsSpan(this BigInteger number, char[] buffer)
        {
            Span<char> bigValue = buffer;
            int charsWritten;
            if (!number.TryFormat(bigValue, out charsWritten))
                throw new InvalidOperationException("Cannot parse the source digit.");
            return bigValue.Slice(0, charsWritten);
        }
    }
}
