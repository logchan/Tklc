using System;
using NUnit.Framework;
using Tklc.IO;

namespace Tklc.Tests.IO {
    public class BitReaderTests {
        [Test]
        public void Test1() {
            var data = new byte[]
            {
                0b00010010, 0b00111101, 0b11110001
            };

            using (var reader = new BitReader(data)) {
                Assert.AreEqual(0, reader.ReadBits(1));
                Assert.AreEqual(0b1001, reader.ReadBits(4));
                Assert.AreEqual(0, reader.ReadBits(3));
                Assert.AreEqual(0b11101, reader.ReadBits(5));
                Assert.AreEqual(0b1001, reader.ReadBits(6));
                Assert.AreEqual(0b11110, reader.ReadBits(5));
            }
        }

        [Test]
        public void Test2() {
            var data = new byte[]
            {
                0b00010010, 0b00111101, 0b11110001
            };

            using (var reader = new BitReader(data)) {
                Assert.AreEqual(0xF13D12, reader.ReadBits(24));
                Assert.Catch<IndexOutOfRangeException>(() => reader.ReadBits(1));
            }
        }

        [Test]
        public void Test3() {
            var data = new byte[9];
            var rng = new Random();
            for (var i = 0; i < 9; ++i) {
                data[i] = (byte)rng.Next(256);
            }

            using (var reader = new BitReader(data)) {
                for (var i = 0; i < 8; ++i) {
                    var lo = (int)data[i];
                    var hi = (int)data[i + 1];

                    lo = (lo & (((1 << (8 - i)) - 1) << i)) >> i;
                    hi = hi & ((1 << (i + 1)) - 1);
                    var expected = (hi << (8 - i)) | lo;
                    Assert.AreEqual(expected, reader.ReadBits(9));
                }
            }
        }
    }
}