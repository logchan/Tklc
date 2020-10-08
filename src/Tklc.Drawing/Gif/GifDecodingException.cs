using System;

namespace Tklc.Drawing.Gif {
    public class GifDecodingException : Exception {
        public GifDecodingException() {
        }

        public GifDecodingException(string message) : base(message) {
        }
    }
}
