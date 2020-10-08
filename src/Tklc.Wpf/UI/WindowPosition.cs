using System.Windows;

namespace Tklc.Wpf.UI {
    public class WindowPosition {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public WindowState WindowState { get; set; }

        public void Update(Window window) {
            Top = (int)window.Top;
            Left = (int)window.Left;
            Width = (int)window.Width;
            Height = (int)window.Height;
            WindowState = window.WindowState;
        }

        public void Restore(Window window) {
            window.Top = Top;
            window.Left = Left;
            if (Width > 0)
                window.Width = Width;
            if (Height > 0)
                window.Height = Height;
            window.WindowState = WindowState;
        }
    }
}
