using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tklc.Wpf.Media {
    public static class ImagingHelpers {
        public static BitmapSource BlendImage(BitmapSource background, Rect backgroundRect, BitmapSource foreground, Rect foregroundRect) {
            return DrawAndRender(background.PixelWidth, background.PixelHeight, background.DpiX, background.DpiY, dc => {
                dc.DrawImage(background, backgroundRect);
                dc.DrawImage(foreground, foregroundRect);
            });
        }

        public static BitmapSource DrawAndRender(int pixelWidth, int pixelHeight,
            double dpiX, double dpiY, Action<DrawingContext> drawingAction) {
            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            drawingAction(dc);
            dc.Close();

            var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
            rtb.Render(dv);
            return rtb;
        }

        // Adapted from https://social.msdn.microsoft.com/Forums/vstudio/en-US/35db45e3-ebd6-4981-be57-2efd623ea439/wpf-bitmapsource-dpi-change?forum=wpf
        /// <summary>
        /// Convert an image to specified DPI.
        /// </summary>
        /// <param name="image">The image to convert</param>
        /// <param name="dpiX">Dpi X</param>
        /// <param name="dpiY">Dpi Y</param>
        public static BitmapSource ConvertToDpi(BitmapSource image, double dpiX, double dpiY) {
            var width = image.PixelWidth;
            var height = image.PixelHeight;

            var stride = width * image.Format.BitsPerPixel / 8;
            var pixelData = new byte[stride * height];

            image.CopyPixels(pixelData, stride, 0);

            return BitmapSource.Create(width, height, dpiX, dpiY, image.Format, image.Palette, pixelData, stride);
        }

        public static ImageSource ZoomTo(BitmapSource image, int width, int height, out double appliedZoom) {
            appliedZoom = width / image.Width;
            appliedZoom = Math.Min(appliedZoom, height / image.Height);

            return ZoomBy(image, appliedZoom);
        }

        public static ImageSource ZoomBy(BitmapSource image, double factor) {
            var imgW = image.Width;
            var imgH = image.Height;
            var width = (int)Math.Round(imgW * factor);
            var height = (int)Math.Round(imgH * factor);
            var paddingW = (width - imgW * factor) / 2;
            var paddingH = (height - imgH * factor) / 2;

            return DrawAndRender(width, height, image.DpiX, image.DpiY, dc => {
                dc.DrawImage(image, new Rect(paddingW, paddingH, imgW * factor, imgH * factor));
            });
        }

        public static BitmapSource BitmapFromData(byte[] data) {
            using (var ms = new MemoryStream(data)) {
                var img = new BitmapImage();

                img.BeginInit();
                img.StreamSource = ms;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();

                return img;
            }
        }
    }
}
