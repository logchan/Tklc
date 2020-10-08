using System.Collections.Generic;
using System.IO;

namespace Tklc.Drawing.Gif {
    public sealed class GifFile {
        public GifVersion Version { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public GifColorTable GlobalColorTable { get; set; }
        public int BackgroundColorIndex { get; set; }
        public double PixelAspectRatio { get; set; }
        public List<GifFrame> Frames { get; } = new List<GifFrame>();
        public List<ApplicationExtensionBlock> ApplicationExtensionBlocks { get; } = new List<ApplicationExtensionBlock>();
        public List<CommentExtensionBlock> CommentExtensionBlocks { get; } = new List<CommentExtensionBlock>();

        /// <summary>
        /// Read a Gif file.
        /// </summary>
        /// <param name="path">Path to a Gif file.</param>
        public GifFile(string path) {
            using (var br = new BinaryReader(File.OpenRead(path))) {
                Read(br);
            }
        }

        /// <summary>
        /// Read a Gif stream.
        /// </summary>
        /// <param name="br">BinaryReader to a Gif stream.</param>
        public GifFile(BinaryReader br) {
            Read(br);
        }

        private void Read(BinaryReader br) {
            ReadSignature(br);
            ReadVersion(br);
            ReadScreenConfig(br);
            ReadGlobalColorTable(br);
            ReadBackgroundColorIndex(br);
            ReadAspectRatio(br);

            GlobalColorTable?.ReadTable(br);

            GraphicsControlExtensionBlock gcb = null;
            do {
                var b = br.ReadByte();
                if (b == 0x2C) {
                    var frame = new GifFrame {
                        ControlExtension = gcb
                    };
                    frame.Read(br);
                    gcb = null;
                    Frames.Add(frame);
                }
                else if (b == 0x21) {
                    var label = br.ReadByte();
                    if (label == 0xF9) {
                        gcb = new GraphicsControlExtensionBlock(br);
                    }
                    else if (label == 0x01) {
                        var frame = new GifFrame {
                            PlainTextExtension = new PlainTextExtensionBlock(br),
                            ControlExtension = gcb
                        };
                        gcb = null;
                        Frames.Add(frame);
                    }
                    else if (label == 0xFF) {
                        ApplicationExtensionBlocks.Add(new ApplicationExtensionBlock(br));
                    }
                    else if (label == 0xFE) {
                        CommentExtensionBlocks.Add(new CommentExtensionBlock(br));
                    }
                    else {
                        throw new GifDecodingException($"Unknown extension label {label:X2}");
                    }
                }
                else if (b == 0x3B) {
                    break;
                }
                else {
                    throw new GifDecodingException($"Unknown separator {b:X2}");
                }
            } while (true);
        }

        private void ReadSignature(BinaryReader br) {
            var signature = br.ReadBytes(3);
            if (signature[0] != 0x47 ||
                signature[1] != 0x49 ||
                signature[2] != 0x46) {
                throw new GifDecodingException("Signature is not GIF");
            }
        }

        private void ReadVersion(BinaryReader br) {
            var version = br.ReadBytes(3);
            if (version[0] == 0x38 && version[2] == 0x61) {
                if (version[1] == 0x37) {
                    Version = GifVersion.Gif87A;
                }
                else if (version[1] == 0x39) {
                    Version = GifVersion.Gif89A;
                }
                else {
                    throw new GifDecodingException("Unknown version");
                }
            }
            else {
                throw new GifDecodingException("Unknown version");
            }
        }

        private void ReadScreenConfig(BinaryReader br) {
            ScreenWidth = br.ReadUInt16();
            ScreenHeight = br.ReadUInt16();
        }

        private void ReadBackgroundColorIndex(BinaryReader br) {
            BackgroundColorIndex = br.ReadByte();
        }

        private void ReadAspectRatio(BinaryReader br) {
            var ratio = br.ReadByte();
            PixelAspectRatio = ratio > 0 ? (ratio + 15) / 64.0 : 1;
        }

        private void ReadGlobalColorTable(BinaryReader br) {
            var b = br.ReadByte();

            if ((b & 0b10000000) != 0) {
                var entryBits = (b & 0b111) + 1;
                var sortFlag = (b & 0b1000) != 0;
                GlobalColorTable = new GifColorTable(entryBits, sortFlag);
            }
        }

        public override string ToString() {
            return
                $"{Version},{ScreenWidth},{ScreenHeight}," +
                $"global={GlobalColorTable != null},bg={BackgroundColorIndex},ar={PixelAspectRatio}," +
                $"frames={Frames.Count}";
        }
    }
}
