using System.IO;

namespace Tklc.Drawing.Gif {
    public sealed class CommentExtensionBlock {
        public byte[] Data { get; set; }

        public CommentExtensionBlock(BinaryReader br) {
            Data = GifHelper.ReadDataBlocks(br);
        }
    }
}
