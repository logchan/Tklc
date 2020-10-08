using System;
using System.IO;

namespace Tklc.IO {
    /// <summary>
    /// Write data by bits.
    /// </summary>
    public class BitWriter : IDisposable {
        private static readonly int[] Masks =
            {0, 0b1, 0b11, 0b111, 0b1111, 0b11111, 0b111111, 0b1111111, 0b11111111};

        private readonly Stream _stream;
        private int _bitPosition = 0;
        private int _current;

        public BitWriter(Stream stream) {
            _stream = stream;
        }

        public void WriteBits(int bits, int length) {
            if (length < 0 || length > 32) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            while (length > 0) {
                var write = Math.Min(length, 8 - _bitPosition);
                length -= write;
                _current = _current | ((bits & Masks[write]) << _bitPosition);
                bits >>= write;

                _bitPosition += write;
                if (_bitPosition == 8) {
                    Flush();
                }
            }
        }

        public void Flush() {
            if (_bitPosition > 0) {
                _stream.WriteByte((byte)_current);
                _current = 0;
                _bitPosition = 0;
            }
        }

        public void Dispose() {
            _stream?.Dispose();
        }
    }
}
