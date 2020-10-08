using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Tklc.Drawing {
    public static class DrawingHelpers {
        /// <summary>
        /// Common image extensions supported by <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        public static string[] CommonImageExtensions => new[]
            { ".bmp", ".png", ".gif", ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".jfi", ".tiff", ".tif", ".ico" };

        /// <summary>
        /// Resize an image to specified width and height, preserving its aspect ratio.
        /// </summary>
        /// <param name="image">The image to be resized.</param>
        /// <param name="maxWidth">The maximum width after resizing.</param>
        /// <param name="maxHeight">The maximum height after resizing.</param>
        /// <returns>Resized image</returns>
        public static Bitmap ResizeImage(Image image, int maxWidth, int maxHeight) {
            var factor = Math.Min(maxWidth / (double)image.Width, maxHeight / (double)image.Height);
            var width = Convert.ToInt32(image.Width * factor);
            var height = Convert.ToInt32(image.Height * factor);

            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Save an image to an JPEG stream with specified quality parameter.
        /// </summary>
        /// <param name="image">The image to be saved.</param>
        /// <param name="stream">Destination stream.</param>
        /// <param name="quality">JPEG quality parameter.</param>
        public static void SaveToJpeg(this Image image, Stream stream, int quality = 80) {
            var param = new EncoderParameters(1) {
                Param =
                {
                    [0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality)
                }
            };

            image.Save(stream, ImageCodecInfo.GetImageDecoders().FirstOrDefault(enc => enc.FormatID == ImageFormat.Jpeg.Guid), param);
        }
    }
}
