using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using MouseRecorder.Native;

namespace MouseRecorder.Services
{
    public class HotkeyService : IDisposable
    {
        private IntPtr _hwnd;
        private HwndSource _source;
        private readonly Dictionary<int, string> _hotkeyToMacro = new Dictionary<int, string>();
        private int _nextId = 1;
        private const int STOP_HOTKEY_ID = 9999;

        public event Action<string> MacroHotkeyPressed;
        public event Action StopHotkeyPressed;

        public static readonly List<string> AvailableHotkeys = new List<string>
        {
            "(None)",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11",
            "Ctrl+F1", "Ctrl+F2", "Ctrl+F3", "Ctrl+F4", "Ctrl+F5",
            "Ctrl+F6", "Ctrl+F7", "Ctrl+F8", "Ctrl+F9", "Ctrl+F10", "Ctrl+F11", "Ctrl+F12",
            "Ctrl+1", "Ctrl+2", "Ctrl+3", "Ctrl+4", "Ctrl+5",
            "Ctrl+6", "Ctrl+7", "Ctrl+8", "Ctrl+9",
        };

        public void Initialize(IntPtr hwnd)
        {
            _hwnd = hwnd;
            _source = HwndSource.FromHwnd(hwnd);
            _source.AddHook(WndProc);

            // Register F12 as emergency stop
            Win32.RegisterHotKey(_hwnd, STOP_HOTKEY_ID, Win32.MOD_NONE, 0x7B);
        }

        public bool RegisterMacroHotkey(string macroId, string hotkeyStr)
        {
            UnregisterMacroHotkey(macroId);

            if (string.IsNullOrEmpty(hotkeyStr) || hotkeyStr == "(None)")
                return true;

            ParseHotkey(hotkeyStr, out uint modifiers, out uint vk);
            if (vk == 0) return false;

            int id = _nextId++;
            if (Win32.RegisterHotKey(_hwnd, id, modifiers, vk))
            {
                _hotkeyToMacro[id] = macroId;
                return true;
            }
            return false;
        }

        public void UnregisterMacroHotkey(string macroId)
        {
            var toRemove = _hotkeyToMacro
                .Where(kvp => kvp.Value == macroId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in toRemove)
            {
                Win32.UnregisterHotKey(_hwnd, id);
                _hotkeyToMacro.Remove(id);
            }
        }

        public void UnregisterAll()
        {
            foreach (var id in _hotkeyToMacro.Keys.ToList())
                Win32.UnregisterHotKey(_hwnd, id);
            _hotkeyToMacro.Clear();

            Win32.UnregisterHotKey(_hwnd, STOP_HOTKEY_ID);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32.WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == STOP_HOTKEY_ID)
                {
                    StopHotkeyPressed?.Invoke();
                    handled = true;
                }
                else if (_hotkeyToMacro.TryGetValue(id, out string macroId))
                {
                    MacroHotkeyPressed?.Invoke(macroId);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void ParseHotkey(string hotkey, out uint modifiers, out uint vk)
        {
            modifiers = Win32.MOD_NONE;
            vk = 0;
            if (string.IsNullOrEmpty(hotkey)) return;

            var parts = hotkey.Split('+');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                switch (trimmed.ToLowerInvariant())
                {
                    case "ctrl":
                    case "control":
                        modifiers |= Win32.MOD_CONTROL;
                        break;
                    case "alt":
                        modifiers |= Win32.MOD_ALT;
                        break;
                    case "shift":
                        modifiers |= Win32.MOD_SHIFT;
                        break;
                    default:
                        vk = KeyMapper.GetVirtualKeyCode(trimmed);
                        break;
                }
            }
        }

        public void Dispose()
        {
            UnregisterAll();
            _source?.RemoveHook(WndProc);
        }
    }
}
