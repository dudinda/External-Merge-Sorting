using System.Numerics;

namespace ExtSort.Code.Extensions
{
    internal static class MemoryExtensions
    {
        public static bool TryParsePriority(this Memory<char> input, out (ReadOnlyMemory<char> Str, BigInteger Int) result) 
        {
            result = default;
            if (input.IsEmpty)
                return false;

            var span = input.Span;
            var idx = span.IndexOf('.');
            if (idx == -1)
                return false;

            if (BigInteger.TryParse(span.Slice(0, idx), out result.Int)) 
            {
                result.Str = input.Slice(idx + 1);
                return true;
            }

            return false;
        }

        public static Memory<char> Eclipse(this Memory<char> input, int maxLength)
        {
            if (input.IsEmpty) return input;
            if (input.Length <= maxLength) return input;
            const string eclipse = "...";
            var firstPart = input.Slice(0, maxLength / 2);
            var lastPart = input.Slice(input.Length - maxLength / 2);
            var buffer = new char[firstPart.Length + eclipse.Length + lastPart.Length].AsMemory();

            var index = 0;
            firstPart.CopyTo(buffer.Slice(index, firstPart.Length));
            index += firstPart.Length;
            eclipse.AsMemory().CopyTo(buffer.Slice(index, eclipse.Length));
            index += eclipse.Length;
            lastPart.CopyTo(buffer.Slice(index, lastPart.Length));

            return buffer;
        }
    }
}
