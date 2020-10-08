using System;
using System.IO;

namespace Tklc.Drawing.Gif {
    public sealed class GifFrame {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public GifColorTable LocalColorTable { get; set; }
        public GraphicsControlExtensionBlock ControlExtension { get; set; }
        public byte[] Data { get; set; }
        public PlainTextExtensionBlock PlainTextExtension { get; set; }

        public void Read(BinaryReader br) {
            Left = br.ReadUInt16();
            Top = br.ReadUInt16();
            Width = br.ReadUInt16();
            Height = br.ReadUInt16();

            var interlace = ReadPacked(br);
            LocalColorTable?.ReadTable(br);
            ReadImage(br, interlace);
        }

        private bool ReadPacked(BinaryReader br) {
            var b = br.ReadByte();

            if ((b & 0b10000000) != 0) {
                var entrySize = (b & 0b111) + 1;
                var sortFlag = (b & 0b100000) != 0;
                LocalColorTable = new GifColorTable(entrySize, sortFlag);
            }

            return (b & 0b1000000) != 0;
        }

        private void ReadImage(BinaryReader br, bool interlace) {
            var codeSize = br.ReadByte();
            var data = GifHelper.ReadDataBlocks(br);
            var decoded = new GifLzwDecoder().Decode(data, codeSize);
            Data = interlace ? ProcessInterlace(decoded) : decoded;
        }

        private byte[] ProcessInterlace(byte[] data) {
            var width = Width;
            var height = Height;
            var result = new byte[data.Length];
            var idx = 0;

            for (var y = 0; y < height; y += 8) {
                Array.Copy(data, width * idx, result, width * y, width);
                ++idx;
            }
            for (var y = 4; y < height; y += 8) {
                Array.Copy(data, width * idx, result, width * y, width);
                ++idx;
            }
            for (var y = 2; y < height; y += 4) {
                Array.Copy(data, width * idx, result, width * y, width);
                ++idx;
            }
            for (var y = 1; y < height; y += 2) {
                Array.Copy(data, width * idx, result, width * y, width);
                ++idx;
            }

            return result;
        }
    }
}
