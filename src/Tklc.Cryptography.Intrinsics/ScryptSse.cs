using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Tklc.Cryptography {
    /// <summary>
    /// Scrypt implementation with SSE, adapted from <see href="https://www.tarsnap.com/scrypt.html"/>.<br/>
    /// </summary>
    public static class ScryptSse {
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
            var v = new Vector128<uint>[iterations * r * 8];
            var x = new uint[r * 32];

            for (var k = 0; k < 2 * r; k++) {
                for (var i = 0; i < 16; i++) {
                    x[k * 16 + i] = Le32Dec(block, (k * 16 + i * 5 % 16) * 4);
                }
            }

            var xs = new Vector128<uint>[r * 8];
            var ys = new Vector128<uint>[r * 8];
            for (var i = 0; i < r * 8; ++i) {
                xs[i] = Vector128.Create(x[i * 4], x[i * 4 + 1], x[i * 4 + 2], x[i * 4 + 3]);
                ys[i] = Vector128.Create(0u);
            }
            
            for (var i = 0; i < iterations; i += 2) {
                for (var j = 0; j < r * 8; ++j) {
                    v[i * r * 8 + j] = xs[j];
                }
                MixSalsa8Sse2(xs, ys, r);

                for (var j = 0; j < r * 8; ++j) {
                    v[(i + 1) * r * 8 + j] = ys[j];
                }
                MixSalsa8Sse2(ys, xs, r);
            }

            for (var i = 0; i < iterations; i += 2) {
                var offset = Integerify(xs, r) & (ulong)(iterations - 1);

                for (var j = 0; j < r * 8; ++j) {
                    xs[j] = Sse2.Xor(xs[j], v[offset * (ulong)r * 8 + (ulong)j]);
                }
                MixSalsa8Sse2(xs, ys, r);

                offset = Integerify(ys, r) & (ulong)(iterations - 1);

                for (var j = 0; j < r * 8; ++j) {
                    ys[j] = Sse2.Xor(ys[j], v[offset * (ulong)r * 8 + (ulong)j]);
                }
                MixSalsa8Sse2(ys, xs, r);
            }

            for (var i = 0; i < r * 8; ++i) {
                for (var j = 0; j < 4; ++j) {
                    x[i * 4 + j] = xs[i].GetElement(j);
                }
            }

            for (var k = 0; k < 2 * r; k++) {
                for (var i = 0; i < 16; i++) {
                    Le32Enc(x[k * 16 + i], block, (k * 16 + i * 5 % 16) * 4);
                }
            }
        }

        private static void MixSalsa8Sse2(Vector128<uint>[] bIn, Vector128<uint>[] bOut, int r) {
            var bTmp = new Vector128<uint>[4];
            for (var j = 0; j < 4; ++j) {
                bTmp[j] = bIn[8 * r - (4 - j)];
            }

            for (var i = 0; i < r; ++i) {
                for (var j = 0; j < 4; ++j) {
                    bTmp[j] = Sse2.Xor(bTmp[j], bIn[i * 8 + j]);
                }
                Salsa8Sse2(bTmp);

                for (var j = 0; j < 4; ++j) {
                    bOut[i * 4 + j] = bTmp[j];
                }

                for (var j = 0; j < 4; ++j) {
                    bTmp[j] = Sse2.Xor(bTmp[j], bIn[i * 8 + 4 + j]);
                }
                Salsa8Sse2(bTmp);

                for (var j = 0; j < 4; ++j) {
                    bOut[i * 4 + r * 4 + j] = bTmp[j];
                }
            }
        }

        private static void Salsa8Sse2(Vector128<uint>[] blocks) {
            var x0 = blocks[0];
            var x1 = blocks[1];
            var x2 = blocks[2];
            var x3 = blocks[3];

            for (var i = 0; i < 8; i += 2) {
                var t = Sse2.Add(x0, x3);
                x1 = Sse2.Xor(x1, Sse2.ShiftLeftLogical(t, 7));
                x1 = Sse2.Xor(x1, Sse2.ShiftRightLogical(t, 25));
                t = Sse2.Add(x1, x0);
                x2 = Sse2.Xor(x2, Sse2.ShiftLeftLogical(t, 9));
                x2 = Sse2.Xor(x2, Sse2.ShiftRightLogical(t, 23));
                t = Sse2.Add(x2, x1);
                x3 = Sse2.Xor(x3, Sse2.ShiftLeftLogical(t, 13));
                x3 = Sse2.Xor(x3, Sse2.ShiftRightLogical(t, 19));
                t = Sse2.Add(x3, x2);
                x0 = Sse2.Xor(x0, Sse2.ShiftLeftLogical(t, 18));
                x0 = Sse2.Xor(x0, Sse2.ShiftRightLogical(t, 14));

                x1 = Sse2.Shuffle(x1, 0x93);
                x2 = Sse2.Shuffle(x2, 0x4E);
                x3 = Sse2.Shuffle(x3, 0x39);

                t = Sse2.Add(x0, x1);
                x3 = Sse2.Xor(x3, Sse2.ShiftLeftLogical(t, 7));
                x3 = Sse2.Xor(x3, Sse2.ShiftRightLogical(t, 25));
                t = Sse2.Add(x3, x0);
                x2 = Sse2.Xor(x2, Sse2.ShiftLeftLogical(t, 9));
                x2 = Sse2.Xor(x2, Sse2.ShiftRightLogical(t, 23));
                t = Sse2.Add(x2, x3);
                x1 = Sse2.Xor(x1, Sse2.ShiftLeftLogical(t, 13));
                x1 = Sse2.Xor(x1, Sse2.ShiftRightLogical(t, 19));
                t = Sse2.Add(x1, x2);
                x0 = Sse2.Xor(x0, Sse2.ShiftLeftLogical(t, 18));
                x0 = Sse2.Xor(x0, Sse2.ShiftRightLogical(t, 14));

                x1 = Sse2.Shuffle(x1, 0x39);
                x2 = Sse2.Shuffle(x2, 0x4E);
                x3 = Sse2.Shuffle(x3, 0x93);
            }

            blocks[0] = Sse2.Add(blocks[0], x0);
            blocks[1] = Sse2.Add(blocks[1], x1);
            blocks[2] = Sse2.Add(blocks[2], x2);
            blocks[3] = Sse2.Add(blocks[3], x3);
        }

        private static ulong Integerify(Vector128<uint>[] blocks, int r) {
            return ((ulong) blocks[(2 * r - 1) * 4 + 3].GetElement(1) << 32) + blocks[(2 * r - 1) * 4].GetElement(0);
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