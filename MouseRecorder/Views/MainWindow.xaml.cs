using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using MouseRecorder.Services;
using MouseRecorder.ViewModels;

namespace MouseRecorder.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;
        private HotkeyService _hotkeyService;
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private bool _isExiting;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainViewModel();
            DataContext = _vm;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hotkeyService = new HotkeyService();
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            _hotkeyService.Initialize(hwnd);
            _vm.SetHotkeyService(_hotkeyService);

            SetupTrayIcon();
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
                overflowGrid.Visibility = Visibility.Collapsed;
        }

        // ── Tray Icon ───────────────────────────────────────────

        private void SetupTrayIcon()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.Text = "Mouse Recorder";

            try
            {
                _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            catch { }

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Show", null, (s, ev) => RestoreFromTray());
            menu.Items.Add("Stop Macro", null, (s, ev) => _vm.StopMacro());
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, ev) => ExitApp());
            _trayIcon.ContextMenuStrip = menu;

            _trayIcon.DoubleClick += (s, ev) => RestoreFromTray();
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            _trayIcon.Visible = false;
        }

        private void ExitApp()
        {
            _isExiting = true;
            _vm.Cleanup();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _trayIcon.Visible = true;
                _trayIcon.ShowBalloonTip(1500, "Mouse Recorder",
                    "Minimized to tray. Hotkeys are still active.", System.Windows.Forms.ToolTipIcon.Info);
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
                return;
            }
            base.OnClosing(e);
        }

        // ── Macro toolbar ───────────────────────────────────────

        private void AddMacro_Click(object sender, RoutedEventArgs e) => _vm.AddMacro();
        private void DeleteMacro_Click(object sender, RoutedEventArgs e) => _vm.DeleteMacro();

        private void PlayMacro_Click(object sender, RoutedEventArgs e) => _vm.PlaySelectedMacro();
        private void StopMacro_Click(object sender, RoutedEventArgs e) => _vm.StopMacro();

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON files|*.json|All files|*.*",
                Title = "Import Macros"
            };
            if (dlg.ShowDialog() == true)
                _vm.ImportMacros(dlg.FileName);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "JSON files|*.json",
                Title = "Export Macros",
                FileName = "macros.json"
            };
            if (dlg.ShowDialog() == true)
                _vm.ExportMacros(dlg.FileName);
        }

        // ── Step buttons ────────────────────────────────────────

        private async void AddClickStep_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMacro == null) return;

            Hide();
            await Task.Delay(350);

            var overlay = new OverlayWindow();
            if (overlay.ShowDialog() == true)
                _vm.AddClickStep(overlay.SelectedX, overlay.SelectedY);

            Show();
            Activate();
        }

        private async void AddDoubleClickStep_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMacro == null) return;

            Hide();
            await Task.Delay(350);

            var overlay = new OverlayWindow();
            if (overlay.ShowDialog() == true)
                _vm.AddDoubleClickStep(overlay.SelectedX, overlay.SelectedY);

            Show();
            Activate();
        }

        private async void AddRightClickStep_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMacro == null) return;

            Hide();
            await Task.Delay(350);

            var overlay = new OverlayWindow();
            if (overlay.ShowDialog() == true)
                _vm.AddRightClickStep(overlay.SelectedX, overlay.SelectedY);

            Show();
            Activate();
        }

        private void AddShortcutStep_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMacro == null) return;

            var dlg = new KeyCaptureWindow { Owner = this };
            if (dlg.ShowDialog() == true && dlg.CapturedKeys.Count > 0)
                _vm.AddKeyStep(dlg.CapturedKeys);
        }

        private void AddKeystrokeStep_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMacro == null) return;
            _vm.AddKeystrokeStep("Enter");
        }

        private void AddTextStep_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMacro == null) return;
            _vm.AddTextStep();
        }

        private void AddWaitStep_Click(object sender, RoutedEventArgs e) => _vm.AddWaitStep();
        private void MoveStepUp_Click(object sender, RoutedEventArgs e) => _vm.MoveStepUp();
        private void MoveStepDown_Click(object sender, RoutedEventArgs e) => _vm.MoveStepDown();
        private void DeleteStep_Click(object sender, RoutedEventArgs e) => _vm.DeleteSelectedStep();

        private async void PickPosition_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedStep == null) return;

            Hide();
            await Task.Delay(350);

            var overlay = new OverlayWindow();
            if (overlay.ShowDialog() == true)
                _vm.UpdateClickStepPosition(overlay.SelectedX, overlay.SelectedY);

            Show();
            Activate();
        }

        private void KeystrokeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo?.SelectedItem is string keyName && _vm.SelectedStep != null)
                _vm.UpdateKeystrokeStep(keyName);
        }

        private void RecaptureKeys_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedStep == null) return;

            var dlg = new KeyCaptureWindow { Owner = this };
            if (dlg.ShowDialog() == true && dlg.CapturedKeys.Count > 0)
                _vm.UpdateKeyStep(dlg.CapturedKeys);
        }

        // ── Inline note editing ──────────────────────────────────

        private void StepNote_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return;

            var grid = sender as Grid;
            if (grid == null) return;

            foreach (var child in grid.Children)
            {
                if (child is TextBlock tb) tb.Visibility = Visibility.Collapsed;
                if (child is TextBox tx)
                {
                    tx.Visibility = Visibility.Visible;
                    tx.Focus();
                    tx.SelectAll();
                }
            }

            e.Handled = true;
        }

        private void StepNoteEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitNoteEdit(sender as TextBox);
        }

        private void StepNoteEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                CommitNoteEdit(sender as TextBox);
                e.Handled = true;
            }
        }

        private void CommitNoteEdit(TextBox textBox)
        {
            if (textBox == null) return;

            var grid = textBox.Parent as Grid;
            if (grid == null) return;

            foreach (var child in grid.Children)
            {
                if (child is TextBlock tb) tb.Visibility = Visibility.Visible;
            }
            textBox.Visibility = Visibility.Collapsed;
            _vm.AutoSave();
        }

        // ── Auto-save on field changes ──────────────────────────

        private void MacroField_LostFocus(object sender, RoutedEventArgs e) => _vm.AutoSave();
        private void StepField_LostFocus(object sender, RoutedEventArgs e) => _vm.AutoSave();
    }
}
