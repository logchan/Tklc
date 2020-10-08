using System.Windows.Media.Imaging;
using Tklc.Drawing.Gif;

namespace Tklc.Wpf.Media {
    public class GifImageFrame {
        public BitmapSource Image { get; set; }
        public int Delay { get; set; }

        public GifFrame Frame { get; set; }
    }
}
