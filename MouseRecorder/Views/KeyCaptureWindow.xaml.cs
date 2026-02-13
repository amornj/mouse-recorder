using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MouseRecorder.Views
{
    public partial class KeyCaptureWindow : Window
    {
        public List<string> CapturedKeys { get; private set; } = new List<string>();

        public KeyCaptureWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var keys = new List<string>();

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) keys.Add("Ctrl");
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) keys.Add("Alt");
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) keys.Add("Shift");

            // Get the actual key (not the modifier)
            var mainKey = e.Key == Key.System ? e.SystemKey : e.Key;

            // Skip if only a modifier was pressed
            if (mainKey == Key.LeftCtrl || mainKey == Key.RightCtrl ||
                mainKey == Key.LeftAlt || mainKey == Key.RightAlt ||
                mainKey == Key.LeftShift || mainKey == Key.RightShift ||
                mainKey == Key.LWin || mainKey == Key.RWin)
            {
                e.Handled = true;
                return;
            }

            keys.Add(ConvertKeyName(mainKey));

            CapturedKeys = keys;
            CapturedDisplay.Text = string.Join(" + ", keys);
            OkButton.IsEnabled = true;

            e.Handled = true;
        }

        private string ConvertKeyName(Key key)
        {
            switch (key)
            {
                case Key.Return: return "Enter";
                case Key.Back: return "Backspace";
                case Key.OemMinus: return "-";
                case Key.OemPlus: return "=";
                case Key.OemOpenBrackets: return "[";
                case Key.OemCloseBrackets: return "]";
                case Key.OemPipe: return "\\";
                case Key.OemSemicolon: return ";";
                case Key.OemQuotes: return "'";
                case Key.OemComma: return ",";
                case Key.OemPeriod: return ".";
                case Key.OemQuestion: return "/";
                case Key.OemTilde: return "`";
                default:
                    var name = key.ToString();
                    // Convert "D0"-"D9" to "0"-"9"
                    if (name.Length == 2 && name[0] == 'D' && char.IsDigit(name[1]))
                        return name[1].ToString();
                    return name;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
