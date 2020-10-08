using System.IO;

namespace Tklc.Drawing.Gif {
    public sealed class GraphicsControlExtensionBlock {
        public bool TransparentColor { get; set; }
        public bool UserInput { get; set; }
        public int DisposalMethod { get; set; }
        public int DelayTime { get; set; }
        public int TransparentColorIndex { get; set; }

        public GraphicsControlExtensionBlock(BinaryReader br) {
            br.ReadByte(); // block size
            ReadPacked(br);
            DelayTime = br.ReadUInt16();
            TransparentColorIndex = br.ReadByte();
            br.ReadByte(); // terminator
        }

        private void ReadPacked(BinaryReader br) {
            var b = br.ReadByte();
            TransparentColor = (b & 1) != 0;
            UserInput = (b & 0b10) != 0;
            DisposalMethod = (b & 0b00011111) >> 2;
        }
    }
}
