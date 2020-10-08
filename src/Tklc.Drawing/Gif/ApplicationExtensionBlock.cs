using System.IO;

namespace Tklc.Drawing.Gif {
    public sealed class ApplicationExtensionBlock {
        public byte[] Identifier { get; set; }
        public byte[] AuthenticationCode { get; set; }
        public byte[] Data { get; set; }

        public ApplicationExtensionBlock(BinaryReader br) {
            br.ReadByte(); // block size, always 11
            Identifier = br.ReadBytes(8);
            AuthenticationCode = br.ReadBytes(3);
            Data = GifHelper.ReadDataBlocks(br);
        }
    }
}
