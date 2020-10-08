using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Tklc.Cryptography {
    /// <summary>
    /// Scrypt implementation adapted from <see href="https://www.tarsnap.com/scrypt.html"/>.<br/>
    /// If you are using .Net Core 3.1 or above, it is recommended to use the ScryptSse class for better performance.
    /// </summary>
    public static class Scrypt {
        private const int BlockSizeUnit = 128;

        public static byte[] Derive(string password, byte[] salt, int mixIterations, int blockSizeFactor, int parallelFactor, int keyLength) {
            var blockSize = BlockSizeUnit * blockSizeFactor;
            var expensiveSalt = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 1, BlockSizeUnit * blockSizeFactor * parallelFactor);

            // mix
            Parallel.For(0, parallelFactor, i => {
                var block = new byte[blockSize];
                Array.Copy(expensiveSalt, blockSize * i, block, 0, blockSize);
                Mix(block, blockSizeFactor, mixIterations);
                Array.Copy(block, 0, expensiveSalt, blockSize * i, blockSize);
            });

            // generate final key
            return KeyDerivation.Pbkdf2(password, expensiveSalt, KeyDerivationPrf.HMACSHA256, 1, keyLength);
        }

        private static void Mix(byte[] block, int r, int iterations) {
            var v = new uint[iterations * r * 32];
            var x = new uint[r * 32];
            var y = new uint[r * 32];

            for (var k = 0; k < r * 32; ++k) {
                x[k] = Le32Dec(block, k * 4);
            }

            for (var i = 0; i < iterations; i += 2) {
                Array.Copy(x, 0, v, i * r * 32, r * 32);
                MixSalsa8(x, y, r);
                Array.Copy(y, 0, v, (i + 1) * r * 32, r * 32);
                MixSalsa8(y, x, r);
            }

            for (var i = 0; i < iterations; i += 2) {
                var j = Integerify(x) & (ulong)(iterations - 1);

                Xor(v, j * (ulong) r * 32, x);
                MixSalsa8(x, y, r);

                j = Integerify(y) & (ulong) (iterations - 1);

                Xor(v, j * (ulong) r * 32, y);
                MixSalsa8(y, x, r);
            }

            for (var k = 0; k < r * 32; ++k) {
                Le32Enc(x[k], block, k * 4);
            }
        }

        private static void MixSalsa8(uint[] bIn, uint[] bOut, int r) {
            var bTmp = new uint[16];
            Array.Copy(bIn, bIn.Length - 16, bTmp, 0, 16);

            for (var i = 0; i < 2 * r; i += 2) {
                Xor(bIn, (ulong) i * 16, bTmp);
                Salsa8(bTmp);

                Array.Copy(bTmp, 0, bOut, i * 8, 16);

                Xor(bIn, (ulong) (i + 1) * 16, bTmp);
                Salsa8(bTmp);

                Array.Copy(bTmp, 0, bOut, i * 8 + r * 16, 16);
            }
        }

        private static void Salsa8(uint[] block) {
            uint R(uint a, int b) {
                return (a << b) | (a >> (32 - b));
            }

            var x = new uint[16];
            Array.Copy(block, 0, x, 0, 16);
            for (var i = 0; i < 8; i += 2) {
                x[4] ^= R(x[0] + x[12], 7);
                x[8] ^= R(x[4] + x[0], 9);
                x[12] ^= R(x[8] + x[4], 13);
                x[0] ^= R(x[12] + x[8], 18);

                x[9] ^= R(x[5] + x[1], 7);
                x[13] ^= R(x[9] + x[5], 9);
                x[1] ^= R(x[13] + x[9], 13);
                x[5] ^= R(x[1] + x[13], 18);

                x[14] ^= R(x[10] + x[6], 7);
                x[2] ^= R(x[14] + x[10], 9);
                x[6] ^= R(x[2] + x[14], 13);
                x[10] ^= R(x[6] + x[2], 18);

                x[3] ^= R(x[15] + x[11], 7);
                x[7] ^= R(x[3] + x[15], 9);
                x[11] ^= R(x[7] + x[3], 13);
                x[15] ^= R(x[11] + x[7], 18);

                x[1] ^= R(x[0] + x[3], 7);
                x[2] ^= R(x[1] + x[0], 9);
                x[3] ^= R(x[2] + x[1], 13);
                x[0] ^= R(x[3] + x[2], 18);

                x[6] ^= R(x[5] + x[4], 7);
                x[7] ^= R(x[6] + x[5], 9);
                x[4] ^= R(x[7] + x[6], 13);
                x[5] ^= R(x[4] + x[7], 18);

                x[11] ^= R(x[10] + x[9], 7);
                x[8] ^= R(x[11] + x[10], 9);
                x[9] ^= R(x[8] + x[11], 13);
                x[10] ^= R(x[9] + x[8], 18);

                x[12] ^= R(x[15] + x[14], 7);
                x[13] ^= R(x[12] + x[15], 9);
                x[14] ^= R(x[13] + x[12], 13);
                x[15] ^= R(x[14] + x[13], 18);
            }

            for (var i = 0; i < 16; i++)
                block[i] += x[i];
        }

        private static ulong Integerify(uint[] x) {
            return ((ulong) x[x.Length - 15] << 32) + x[x.Length - 16];
        }

        private static void Xor(uint[] src, ulong srcOffset, uint[] dst) {
            for (var i = 0; i < dst.Length; ++i) {
                dst[i] = dst[i] ^ src[(ulong)i + srcOffset];
            }
        }

        private static uint Le32Dec(byte[] data, int offset) {
            return data[offset] +
                   ((uint) data[offset + 1] << 8) +
                   ((uint) data[offset + 2] << 16) +
                   ((uint) data[offset + 3] << 24);
        }

        private static void Le32Enc(uint n, byte[] data, int offset) {
            data[offset] = (byte) (n & 0xFF);
            data[offset + 1] = (byte) ((n >> 8) & 0xFF);
            data[offset + 2] = (byte) ((n >> 16) & 0xFF);
            data[offset + 3] = (byte) ((n >> 24) & 0xFF);
        }
    }
}
