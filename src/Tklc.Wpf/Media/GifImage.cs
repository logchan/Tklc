using System.Collections.Generic;
using Tklc.Drawing.Gif;

namespace Tklc.Wpf.Media {
    public class GifImage {
        public int Width { get; set; }
        public int Height { get; set; }
        public GifFile File { get; set; }

        public List<GifImageFrame> Frames { get; } = new List<GifImageFrame>();
    }
}
