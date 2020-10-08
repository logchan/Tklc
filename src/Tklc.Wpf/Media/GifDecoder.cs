using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tklc.Drawing.Gif;

namespace Tklc.Wpf.Media {
    public class GifDecoder {
        public static Color[] GreyscaleColorTable { get; }

        static GifDecoder() {
            GreyscaleColorTable = new Color[256];
            for (var i = 0; i < 256; ++i) {
                var v = (byte)i;
                GreyscaleColorTable[i] = Color.FromRgb(v, v, v);
            }
        }

        public Color[] DefaultColorTable { get; set; } = GreyscaleColorTable;

        public GifImage Decode(BinaryReader br, double dpiX, double dpiY, int defaultDelay = 10) {
            var gif = new GifFile(br);

            // TODO: ratio?
            var image = new GifImage {
                File = gif,
                Width = gif.ScreenWidth,
                Height = gif.ScreenHeight
            };

            var globalColorTable = TransformColor(gif.GlobalColorTable?.Colors) ?? DefaultColorTable;
            var prevBm = ImagingHelpers.DrawAndRender(image.Width, image.Height, dpiX, dpiY,
                dc => {
                    dc.DrawRectangle(new SolidColorBrush(globalColorTable[gif.BackgroundColorIndex]), null,
                        new Rect(0, 0, image.Width, image.Height));
                });
            var dpiFixFactor = prevBm.Width / image.Width;

            foreach (var frame in gif.Frames) {
                var colorTable = TransformColor(frame.LocalColorTable?.Colors) ?? globalColorTable;
                var ctrl = frame.ControlExtension;
                var preserved = Colors.Transparent;
                if (ctrl?.TransparentColor ?? false) {
                    preserved = colorTable[ctrl.TransparentColorIndex];
                    colorTable[ctrl.TransparentColorIndex] = Colors.Transparent;
                }

                var palette = new BitmapPalette(colorTable);

                var bm = BitmapSource.Create(frame.Width, frame.Height, dpiX, dpiY, PixelFormats.Indexed8, palette,
                    frame.Data, frame.Width);

                if (ctrl?.TransparentColor ?? false) {
                    colorTable[ctrl.TransparentColorIndex] = preserved;
                }

                bm = ImagingHelpers.BlendImage(prevBm, new Rect(0, 0, image.Width * dpiFixFactor, image.Height * dpiFixFactor),
                    bm, new Rect(frame.Left * dpiFixFactor, frame.Top * dpiFixFactor, frame.Width * dpiFixFactor, frame.Height * dpiFixFactor));
                prevBm = bm;

                image.Frames.Add(new GifImageFrame {
                    Delay = (ctrl?.DelayTime ?? defaultDelay) * 10,
                    Frame = frame,
                    Image = bm
                });
            }

            return image;
        }

        public GifImage Decode(string path, double dpiX, double dpiY, int defaultDelay = 100) {
            using (var br = new BinaryReader(File.OpenRead(path))) {
                return Decode(br, dpiX, dpiY, defaultDelay);
            }
        }

        private Color[] TransformColor(System.Drawing.Color[] colors) {
            if (colors == null) {
                return null;
            }

            var result = new Color[colors.Length];
            for (var i = 0; i < colors.Length; ++i) {
                var c = colors[i];
                result[i] = Color.FromRgb(c.R, c.G, c.B);
            }

            return result;
        }
    }
}
