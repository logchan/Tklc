using System;
using System.IO;

namespace Tklc.IO {
    /// <summary>
    /// Read data by bits.
    /// </summary>
    public class BitReader : IDisposable {
        public const int MaximumReadLength = 32;

        private static readonly int[] Masks =
            {0, 0b1, 0b11, 0b111, 0b1111, 0b11111, 0b111111, 0b1111111, 0b11111111};

        private readonly Stream _stream;
        private int _bitPosition = 8;
        private byte _current;
        private bool _eos = false;

        public BitReader(Stream stream) {
            _stream = stream;
        }

        public BitReader(byte[] data) : this(new MemoryStream(data)) {

        }

        public int ReadBits(int length) {
            if (length < 1 || length > MaximumReadLength) {
                throw new NotSupportedException(nameof(length));
            }

            if (length > 0 && _eos) {
                throw new IndexOutOfRangeException();
            }

            var result = 0;
            var need = length;
            while (need > 0) {
                var read = Math.Min(need, 8 - _bitPosition);
                result = result | ((_current & Masks[read]) << (length - need));
                need -= read;

                _bitPosition += read;
                if (_bitPosition == 8) {
                    _bitPosition = 0;

                    var b = _stream.ReadByte();
                    if (b >= 0) {
                        _current = (byte)b;
                    }
                    else {
                        _eos = true;
                        if (need > 0) {
                            throw new IndexOutOfRangeException();
                        }
                    }
                }
                else {
                    _current >>= read;
                }
            }

            return result;
        }

        public void Dispose() {
            _stream?.Dispose();
        }
    }
}
