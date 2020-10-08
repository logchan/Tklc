using System.Drawing;
using System.IO;

namespace Tklc.Drawing.Gif {
    public class GifColorTable {
        public int EntryBits { get; set; }
        public bool SortFlag { get; set; }
        public Color[] Colors { get; set; }

        public int Count => 1 << EntryBits;

        public GifColorTable(int entryBits, bool sortFlag) {
            EntryBits = entryBits;
            SortFlag = sortFlag;
            Colors = new Color[Count];
        }

        public void ReadTable(BinaryReader br) {
            for (var i = 0; i < Count; ++i) {
                var r = br.ReadByte();
                var g = br.ReadByte();
                var b = br.ReadByte();

                Colors[i] = Color.FromArgb(r, g, b);
            }
        }
    }
}
