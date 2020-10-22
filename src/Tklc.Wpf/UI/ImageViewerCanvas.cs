using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tklc.Wpf.Media;

namespace Tklc.Wpf.UI {
    /// <summary>
    /// A canvas for displaying images in WPF applications.
    /// </summary>
    public class ImageViewerCanvas : Canvas {
        private const int FrameThickness = 2;
        private const int SelectionThickness = 3;
        private const int MinimumDuration = 10;
        private const int ChessboardWidth = 24;
        private readonly Brush _chessboardBrush;

        private static readonly Regex _digitKeyRegex = new Regex(@"^D(?<digit>[1-9])$");

        private BitmapSource _image;
        private double _fitZoom;
        private Point _scroll;
        private FrameData[] _frames = new FrameData[0];

        public double ZoomStep { get; set; } = 0.1;
        public double MaximumZoom { get; set; } = 64;
        public double ZoomDoubleThreshold { get; set; } = 4;
        public bool AutoRestart { get; set; } = true;
        public double Rotation { get; set; }

        public ImageZoomMode ZoomMode {
            get => (ImageZoomMode)GetValue(ZoomModeProperty);
            set => SetValue(ZoomModeProperty, value);
        }

        public static readonly DependencyProperty ZoomModeProperty =
            DependencyProperty.Register("ZoomMode", typeof(ImageZoomMode), typeof(ImageViewerCanvas), new PropertyMetadata(ImageZoomMode.FitTooLarge));

        public double Zoom {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(ImageViewerCanvas), new PropertyMetadata(1.0));

        public int ImageWidth {
            get => (int)GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(int), typeof(ImageViewerCanvas), new PropertyMetadata(0));

        public int ImageHeight {
            get => (int)GetValue(ImageHeightProperty);
            set => SetValue(ImageHeightProperty, value);
        }

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(int), typeof(ImageViewerCanvas), new PropertyMetadata(0));

        public FrameData[] Frames {
            get => (FrameData[])GetValue(FramesProperty);
            set => SetValue(FramesProperty, value);
        }

        public static readonly DependencyProperty FramesProperty =
            DependencyProperty.Register("Frames", typeof(FrameData[]), typeof(ImageViewerCanvas),
                new PropertyMetadata(new FrameData[0], (obj, args) => {
                    if (!(obj is ImageViewerCanvas canvas) || !(args.NewValue is FrameData[] frames)) {
                        return;
                    }
                    canvas._frames = frames;
                    canvas.ProcessDpi();
                    if (!canvas.KeepTransform) {
                        canvas.SetZoom(-1, true);
                        canvas.Rotation = 0;
                    }

                    canvas.SetFrame(0);
                }));

        public bool KeepTransform {
            get => (bool)GetValue(KeepTransformProperty);
            set => SetValue(KeepTransformProperty, value);
        }

        public static readonly DependencyProperty KeepTransformProperty =
            DependencyProperty.Register("KeepTransform", typeof(bool), typeof(ImageViewerCanvas), new PropertyMetadata(false));

        public bool Chessboard {
            get => (bool)GetValue(ChessboardProperty);
            set => SetValue(ChessboardProperty, value);
        }

        public static readonly DependencyProperty ChessboardProperty =
            DependencyProperty.Register("Chessboard", typeof(bool), typeof(ImageViewerCanvas), new PropertyMetadata(false, InvalidateAfterPropertyChange));

        public bool DrawFrame {
            get => (bool)GetValue(DrawFrameProperty);
            set => SetValue(DrawFrameProperty, value);
        }

        public static readonly DependencyProperty DrawFrameProperty =
            DependencyProperty.Register("DrawFrame", typeof(bool), typeof(ImageViewerCanvas), new PropertyMetadata(false, InvalidateAfterPropertyChange));

        private static void InvalidateAfterPropertyChange(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            if (!(obj is ImageViewerCanvas canvas)) {
                return;
            }

            canvas.InvalidateVisual();
        }

        public ImageViewerCanvas() {
            var cb = GenerateChessboard();
            _chessboardBrush = new ImageBrush(cb) {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, ChessboardWidth * 2, ChessboardWidth * 2),
                Stretch = Stretch.None,
                TileMode = TileMode.Tile
            };
        }

        public void SetImage(BitmapSource image) {
            SetFrames(new[] { new FrameData { Image = image } });
        }

        public void SetFrames(IEnumerable<FrameData> frames) {
            Frames = frames.ToArray();
        }

        private void SetFrame(int idx) {
            if (idx < 0 || idx >= _frames.Length) {
                return;
            }

            var frame = _frames[idx];
            _image = frame.Image;
            ImageWidth = _image.PixelWidth;
            ImageHeight = _image.PixelHeight;

            if (_frames.Length > 1) {
                if (AutoRestart && idx == _frames.Length - 1) {
                    idx = -1;
                }

                if (idx < _frames.Length - 1) {
                    var waitTime = Math.Max(MinimumDuration, frame.Duration);
                    Task.Run(() => {
                        Thread.Sleep(waitTime);
                        Dispatcher.Invoke(() => SetFrame(idx + 1));
                    });
                }
            }

            InvalidateVisual();
        }

        private void SetZoom(double zoom, bool preventUnderZoom) {
            if (zoom > 0 && (!preventUnderZoom || zoom >= _fitZoom)) {
                Zoom = Math.Min(zoom, MaximumZoom);
                ZoomMode = ImageZoomMode.Custom;
            }
            else {
                Zoom = _fitZoom;
                ZoomMode = ImageZoomMode.FitTooLarge;
                _scroll.X = _scroll.Y = 0;
            }
        }

        private void CenterScroll(bool x, bool y) {
            if (x)
                _scroll.X = (ActualWidth - _image.Width * Zoom) / 2;
            if (y)
                _scroll.Y = (ActualHeight - _image.Height * Zoom) / 2;
        }

        protected override void OnRender(DrawingContext dc) {
            if (_image == null) {
                return;
            }

            ProcessDpi();

            dc.DrawRectangle(Chessboard ? _chessboardBrush : Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));

            _fitZoom = Math.Min(Math.Min(ActualWidth / _image.Width, ActualHeight / _image.Height), 1);
            switch (ZoomMode) {
                case ImageZoomMode.FitTooLarge:
                    Zoom = _fitZoom;
                    CenterScroll(true, true);
                    break;
                case ImageZoomMode.Custom:

                    break;
            }

            RenderOptions.SetBitmapScalingMode(this,
                Zoom >= ZoomDoubleThreshold ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality);

            dc.PushTransform(new TranslateTransform(_scroll.X, _scroll.Y));
            dc.PushTransform(new ScaleTransform(Zoom, Zoom));
            dc.PushTransform(new RotateTransform(Rotation, _image.Width / 2.0, _image.Height / 2.0));
            if (DrawFrame) {
                const double offset = FrameThickness / 2.0;
                dc.DrawRectangle(null, new Pen(Brushes.Black, FrameThickness), new Rect(-offset, -offset, FrameThickness + _image.Width, FrameThickness + _image.Height));
            }
            dc.DrawImage(_image, new Rect(0, 0, _image.Width, _image.Height));
            dc.Pop();
            dc.Pop();
            dc.Pop();

            if (_selecting) {
                dc.DrawRectangle(null, new Pen(Brushes.Red, SelectionThickness), new Rect(_mouseStart, _mouseEnd));
            }
        }

        private static ImageSource GenerateChessboard() {
            var w = ChessboardWidth;
            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, w * 2, w * 2));
            dc.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, w, w));
            dc.DrawRectangle(Brushes.LightGray, null, new Rect(w, w, w, w));
            dc.Close();

            var rtb = new RenderTargetBitmap(w * 2, w * 2, 96.0, 96.0, PixelFormats.Pbgra32);
            rtb.Render(dv);
            return rtb;
        }

        private void ProcessDpi() {
            var dpiMat = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice;
            var dpiX = 96.0;
            var dpiY = 96.0;
            if (dpiMat.HasValue) {
                dpiX = dpiMat.Value.M11 * 96;
                dpiY = dpiMat.Value.M22 * 96;
            }

            foreach (var frame in _frames) {
                if (Math.Abs(dpiX - frame.Image.DpiX) >= 1 ||
                    Math.Abs(dpiY - frame.Image.DpiY) >= 1) {
                    frame.Image = ImagingHelpers.ConvertToDpi(frame.Image, dpiX, dpiY);
                }
            }
        }

        private Point _mouseStart;
        private Point _mouseEnd;
        private Point _preservedScroll;
        private bool _dragging;
        private bool _selecting;

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            _preservedScroll = _scroll;
            _mouseStart = e.GetPosition(this);
            _mouseEnd = _mouseStart;
            _dragging = !IsCtrlAlone();
            _selecting = !_dragging;
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            _dragging = false;
            if (_selecting) {
                ApplySelection();
                _selecting = false;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            _dragging = false;
            if (_selecting) {
                ApplySelection();
                _selecting = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (_dragging) {
                _mouseEnd = e.GetPosition(this);
                _scroll.X = _preservedScroll.X + _mouseEnd.X - _mouseStart.X;
                _scroll.Y = _preservedScroll.Y + _mouseEnd.Y - _mouseStart.Y;
                InvalidateVisual();
                return;
            }

            if (_selecting) {
                _mouseEnd = e.GetPosition(this);
                InvalidateVisual();
            }
        }

        private Point MousePointToImagePoint(Point point) {
            point.X = (point.X - _scroll.X) / Zoom;
            point.Y = (point.Y - _scroll.Y) / Zoom;
            return point;
        }

        private void ApplySelection() {
            ZoomMode = ImageZoomMode.Custom;

            var start = MousePointToImagePoint(_mouseStart);
            var end = MousePointToImagePoint(_mouseEnd);
            var w = Math.Abs(start.X - end.X);
            var h = Math.Abs(start.Y - end.Y);
            Zoom = Math.Min(Math.Min(ActualWidth / w, ActualHeight / h), MaximumZoom);

            _scroll.X = ActualWidth / 2 - (Math.Min(start.X, end.X) + w / 2) * Zoom;
            _scroll.Y = ActualHeight / 2 - (Math.Min(start.Y, end.Y) + h / 2) * Zoom;

            InvalidateVisual();
        }

        private bool IsCtrlAlone() {
            return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                !(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) &&
                !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            if (IsCtrlAlone()) {
                // ensure that the point under the mouse pointer is not changed
                // user experience!
                var zoom = Zoom;
                var previousZoom = zoom;
                var scaleFactor = e.GetPosition(this);
                scaleFactor.X = (scaleFactor.X - _scroll.X) / (_image.Width * zoom);
                scaleFactor.Y = (scaleFactor.Y - _scroll.Y) / (_image.Height * zoom);

                if (e.Delta > 0) {
                    zoom = zoom < ZoomDoubleThreshold ? zoom + ZoomStep : zoom * 2;
                }
                else {
                    zoom = zoom > ZoomDoubleThreshold ? zoom / 2 : zoom - ZoomStep;
                }
                SetZoom(zoom, true);

                _scroll.X -= _image.Width * scaleFactor.X * (Zoom - previousZoom);
                _scroll.Y -= _image.Height * scaleFactor.Y * (Zoom - previousZoom);
            }
            else {
                _scroll.Y += e.Delta;
            }

            InvalidateVisual();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (!IsCtrlAlone())
                return;

            // Zoom levels
            var match = _digitKeyRegex.Match(e.Key.ToString());
            if (match.Success) {
                SetZoom(Int32.Parse(match.Groups["digit"].Value), true);

                CenterScroll(true, false);
                InvalidateVisual();
                return;
            }

            switch (e.Key) {
                case Key.D0:
                    SetZoom(-1, true);
                    CenterScroll(true, false);
                    InvalidateVisual();
                    break;
                case Key.C:
                    Clipboard.SetImage(_image);
                    break;
                default:
                    return;
            }
        }
        
        public class FrameData {
            public BitmapSource Image { get; set; }
            public int Duration { get; set; }
        }
    }
}
