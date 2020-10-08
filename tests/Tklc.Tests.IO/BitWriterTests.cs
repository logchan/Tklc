using System;
using System.IO;
using NUnit.Framework;
using Tklc.IO;

namespace Tklc.Tests.IO
{
    public class BitWriterTests
    {
        [Test]
        public void Test1()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BitWriter(ms))
            {
                writer.WriteBits(0b110, 3);
                writer.WriteBits(0b01000, 5);
                writer.WriteBits(0b01, 2);
                writer.WriteBits(0b1110100, 7);
                writer.Flush();

                var data = ms.GetBuffer();
                Assert.AreEqual(0b01000110, data[0]);
                Assert.AreEqual(0b11010001, data[1]);
                Assert.AreEqual(0b1, data[2]);
            }
        }

        [Test]
        public void Test2()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BitWriter(ms))
            {
                writer.WriteBits(0b1, 1);
                writer.WriteBits(0b10010, 5);
                writer.WriteBits(0b10110011010111, 14);
                writer.WriteBits(0b1000, 4);
                writer.Flush();

                var data = ms.GetBuffer();
                var reader = new BitReader(data);
                Assert.AreEqual(0b100010110011010111100101, reader.ReadBits(24));
            }
        }
    }
}
