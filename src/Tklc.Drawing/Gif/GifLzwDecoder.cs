using System;
using System.Collections.Generic;
using System.IO;
using Tklc.IO;

namespace Tklc.Drawing.Gif {
    internal class GifLzwDecoder {
        private const int LzwMaxCode = 4095;
        private const int GifByteSize = 8;

        /// <summary>
        /// Decode LZW stream in GIF, probably copied from somewhere else but I can't remember. If you believe that I copied your code, contact me and I'll credit you.
        /// </summary>
        public byte[] Decode(BitReader br, int codeSize) {
            using (var ms = new MemoryStream())
            using (var bw = new BitWriter(ms)) {
                var dict = new Tuple<int, int>[LzwMaxCode + 1];
                for (var i = 0; i < (1 << codeSize); ++i) {
                    dict[i] = new Tuple<int, int>(LzwMaxCode + 1, i);
                }

                var initCodeSize = codeSize;
                var clearCode = 1 << codeSize;
                var eofCode = clearCode + 1;

                var prevCode = 0;
                var currDictIdx = 0;
                var currDictMax = 0;

                ++codeSize;
                while (true) {
                    var code = br.ReadBits(codeSize);
                    if (code == clearCode) {
                        codeSize = initCodeSize;
                        currDictIdx = clearCode + 2;

                        ++codeSize;
                        currDictMax = (1 << codeSize);

                        prevCode = br.ReadBits(codeSize);
                        if (prevCode >= (1 << initCodeSize)) {
                            throw new GifDecodingException("no literal after clear");
                        }
                        bw.WriteBits(prevCode, GifByteSize);
                    }
                    else if (code == eofCode) {
                        break;
                    }
                    else {
                        if (code > currDictIdx) {
                            throw new GifDecodingException("code > currDictIdx");
                        }

                        int newItem;
                        if (code < currDictIdx) {
                            newItem = WriteDictString(bw, GifByteSize, dict, code);
                        }
                        else {
                            newItem = WriteDictString(bw, GifByteSize, dict, prevCode);
                            bw.WriteBits(newItem, GifByteSize);
                        }

                        if (currDictIdx < currDictMax) {
                            dict[currDictIdx++] = new Tuple<int, int>(prevCode, newItem);
                        }

                        prevCode = code;
                    }

                    if (currDictIdx == currDictMax) {
                        if (currDictMax < LzwMaxCode) {
                            ++codeSize;
                            currDictMax = (1 << codeSize);
                        }
                    }
                }

                bw.Flush();
                return ms.GetBuffer();
            }
        }

        public byte[] Decode(byte[] data, int codeSize) {
            using (var br = new BitReader(data)) {
                return Decode(br, codeSize);
            }
        }

        private readonly Stack<int> _stack = new Stack<int>(LzwMaxCode + 1);
        private int WriteDictString(BitWriter bw, int length, Tuple<int, int>[] dict, int start) {
            _stack.Clear();
            var stack = _stack;

            var idx = start;
            while (idx < LzwMaxCode + 1) {
                stack.Push(dict[idx].Item2);
                idx = dict[idx].Item1;
            }

            var last = stack.Peek();
            while (stack.Count > 0) {
                bw.WriteBits(stack.Pop(), length);
            }

            return last;
        }
    }
}
