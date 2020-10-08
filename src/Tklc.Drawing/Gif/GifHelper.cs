using System.IO;

namespace Tklc.Drawing.Gif {
    internal static class GifHelper {
        /// <summary>
        /// Read consecutive GIF data blocks.
        /// </summary>
        /// <param name="br">A BinaryReader reading a GIF stream.</param>
        /// <returns>Read data.</returns>
        public static byte[] ReadDataBlocks(BinaryReader br) {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms)) {
                do {
                    var size = br.ReadByte();
                    if (size == 0) {
                        break;
                    }

                    var buffer = br.ReadBytes(size);
                    bw.Write(buffer);
                } while (true);

                return ms.GetBuffer();
            }
        }
    }
}
