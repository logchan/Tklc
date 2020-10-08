using System.IO;

namespace Tklc.Drawing.Gif {
    public sealed class PlainTextExtensionBlock {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int CharacterWidth { get; set; }
        public int CharacterHeight { get; set; }
        public int ForegroundColorIndex { get; set; }
        public int BackgroundColorIndex { get; set; }
        public byte[] Data { get; set; }

        public PlainTextExtensionBlock(BinaryReader br) {
            br.ReadByte(); // block size, always 12
            Left = br.ReadUInt16();
            Top = br.ReadUInt16();
            Width = br.ReadUInt16();
            Height = br.ReadUInt16();
            CharacterWidth = br.ReadByte();
            CharacterHeight = br.ReadByte();
            ForegroundColorIndex = br.ReadByte();
            BackgroundColorIndex = br.ReadByte();
            Data = GifHelper.ReadDataBlocks(br);
        }
    }
}
