using System.Windows;
using System.Windows.Input;
using MouseRecorder.Native;

namespace MouseRecorder.Views
{
    public partial class OverlayWindow : Window
    {
        public int SelectedX { get; private set; }
        public int SelectedY { get; private set; }

        public OverlayWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            // Use WPF coordinates for visual positioning
            var pos = e.GetPosition(Overlay);
            double canvasW = Overlay.ActualWidth;
            double canvasH = Overlay.ActualHeight;

            // Horizontal line
            HLine.X1 = 0;
            HLine.Y1 = pos.Y;
            HLine.X2 = canvasW;
            HLine.Y2 = pos.Y;

            // Vertical line
            VLine.X1 = pos.X;
            VLine.Y1 = 0;
            VLine.X2 = pos.X;
            VLine.Y2 = canvasH;

            // Get real screen coordinates for display
            Win32.POINT screenPt;
            Win32.GetCursorPos(out screenPt);

            CoordText.Text = $"({screenPt.X}, {screenPt.Y})";

            // Position the label offset from cursor
            double labelX = pos.X + 16;
            double labelY = pos.Y + 16;

            // Keep label on screen
            if (labelX + 120 > canvasW) labelX = pos.X - 130;
            if (labelY + 30 > canvasH) labelY = pos.Y - 40;

            Canvas.SetLeft(CoordBorder, labelX);
            Canvas.SetTop(CoordBorder, labelY);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Capture actual screen pixel coordinates
            Win32.POINT pt;
            Win32.GetCursorPos(out pt);
            SelectedX = pt.X;
            SelectedY = pt.Y;
            DialogResult = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }
    }
}
