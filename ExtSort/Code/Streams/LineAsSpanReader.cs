using System.Text;

namespace ExtSort.Code.Streams
{
    internal sealed class LineAsSpanReader : StreamReaderExtendable
    {
        public LineAsSpanReader(Stream stream) : base(stream) { }
        public LineAsSpanReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize) { }

        public Memory<char> ReadLineAsMemory(int transitionOffset = 80)
        {
            ThrowIfDisposed();

            if (_charPos == _charLen)
            {
                if (ReadBuffer() == 0)
                {
                    return null;
                }
            }
            char[]? targetBuffer = null;
            int nullCharIndex = 0;
            int length;
            do
            {
                int i = _charPos;
                do
                {
                    char ch = _charBuffer[i];
                    // Note the following common line feed chars:
                    // \n - UNIX   \r\n - DOS   \r - Mac
                    if (ch == '\r' || ch == '\n')
                    {
                        Memory<char> result;
                        length = i - _charPos;
                        if (targetBuffer != null)
                        {
                            Array.Copy(_charBuffer, _charPos, targetBuffer, nullCharIndex, length);
                            result = targetBuffer.AsMemory().Slice(0, nullCharIndex + length);
                        }
                        else
                        {
                            targetBuffer = new char[length];
                            Array.Copy(_charBuffer, _charPos, targetBuffer, 0, length);
                            result = targetBuffer.AsMemory();

                        }
                        _charPos = i + 1;
                        if (ch == '\r' && (_charPos < _charLen || ReadBuffer() > 0))
                        {
                            if (_charBuffer[_charPos] == '\n')
                            {
                                _charPos++;
                            }
                        }
                        return result;
                    }
                    i++;
                } while (i < _charLen);

                i = _charLen - _charPos;
                targetBuffer = new char[i + transitionOffset];
                Array.Copy(_charBuffer, _charPos, targetBuffer, 0, i);
                nullCharIndex = i;
            } while (ReadBuffer() > 0);
            return targetBuffer.AsMemory();
        }
        public Span<char> ReadLineAsSpan(int transitionOffset = 80)
        {
            ThrowIfDisposed();

            if (_charPos == _charLen)
            {
                if (ReadBuffer() == 0)
                {
                    return null;
                }
            }

            char[]? targetBuffer = null;
            int nullCharIndex = 0;
            int length;
            do
            {
                int i = _charPos;
                do
                {
                    char ch = _charBuffer[i];
                    // Note the following common line feed chars:
                    // \n - UNIX   \r\n - DOS   \r - Mac
                    if (ch == '\r' || ch == '\n')
                    {
                        Span<char> result;
                        length = i - _charPos;
                        if (targetBuffer != null)
                        {
                            Array.Copy(_charBuffer, _charPos, targetBuffer, nullCharIndex, length);
                            result = targetBuffer.AsSpan().Slice(0, nullCharIndex + length);
                        }
                        else
                        {
                            targetBuffer = new char[length];
                            Array.Copy(_charBuffer, _charPos, targetBuffer, 0, length);
                            result = targetBuffer.AsSpan();
                        }
                        _charPos = i + 1;
                        if (ch == '\r' && (_charPos < _charLen || ReadBuffer() > 0))
                        {
                            if (_charBuffer[_charPos] == '\n')
                            {
                                _charPos++;
                            }
                        }
                        return result;
                    }
                    i++;
                } while (i < _charLen);

                i = _charLen - _charPos;
                targetBuffer = new char[i + transitionOffset];
                Array.Copy(_charBuffer, _charPos, targetBuffer, 0, i);
                nullCharIndex = i;
            } while (ReadBuffer() > 0);
            return targetBuffer.AsSpan();
        }
    }
}
