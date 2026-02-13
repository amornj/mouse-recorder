using System;
using System.Collections.Generic;

namespace MouseRecorder.Native
{
    public static class KeyMapper
    {
        private static readonly Dictionary<string, ushort> Map = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase)
        {
            // Modifiers
            {"Ctrl", 0x11}, {"Control", 0x11},
            {"Alt", 0x12}, {"Menu", 0x12},
            {"Shift", 0x10},
            {"Win", 0x5B},

            // Function keys
            {"F1", 0x70}, {"F2", 0x71}, {"F3", 0x72}, {"F4", 0x73},
            {"F5", 0x74}, {"F6", 0x75}, {"F7", 0x76}, {"F8", 0x77},
            {"F9", 0x78}, {"F10", 0x79}, {"F11", 0x7A}, {"F12", 0x7B},

            // Navigation
            {"Enter", 0x0D}, {"Return", 0x0D},
            {"Tab", 0x09},
            {"Escape", 0x1B}, {"Esc", 0x1B},
            {"Space", 0x20},
            {"Backspace", 0x08}, {"Back", 0x08},
            {"Delete", 0x2E}, {"Del", 0x2E},
            {"Insert", 0x2D},
            {"Home", 0x24}, {"End", 0x23},
            {"PageUp", 0x21}, {"PgUp", 0x21},
            {"PageDown", 0x22}, {"PgDn", 0x22},

            // Arrow keys
            {"Up", 0x26}, {"Down", 0x28}, {"Left", 0x25}, {"Right", 0x27},

            // Letters
            {"A", 0x41}, {"B", 0x42}, {"C", 0x43}, {"D", 0x44},
            {"E", 0x45}, {"F", 0x46}, {"G", 0x47}, {"H", 0x48},
            {"I", 0x49}, {"J", 0x4A}, {"K", 0x4B}, {"L", 0x4C},
            {"M", 0x4D}, {"N", 0x4E}, {"O", 0x4F}, {"P", 0x50},
            {"Q", 0x51}, {"R", 0x52}, {"S", 0x53}, {"T", 0x54},
            {"U", 0x55}, {"V", 0x56}, {"W", 0x57}, {"X", 0x58},
            {"Y", 0x59}, {"Z", 0x5A},

            // Numbers
            {"0", 0x30}, {"1", 0x31}, {"2", 0x32}, {"3", 0x33},
            {"4", 0x34}, {"5", 0x35}, {"6", 0x36}, {"7", 0x37},
            {"8", 0x38}, {"9", 0x39},

            // Punctuation / symbols
            {";", 0xBA}, {"=", 0xBB}, {",", 0xBC}, {"-", 0xBD},
            {".", 0xBE}, {"/", 0xBF}, {"`", 0xC0},
            {"[", 0xDB}, {"\\", 0xDC}, {"]", 0xDD}, {"'", 0xDE},

            // Other
            {"PrintScreen", 0x2C}, {"PrtSc", 0x2C},
            {"ScrollLock", 0x91}, {"Pause", 0x13},
            {"NumLock", 0x90}, {"CapsLock", 0x14},
        };

        public static ushort GetVirtualKeyCode(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName)) return 0;
            ushort vk;
            return Map.TryGetValue(keyName.Trim(), out vk) ? vk : (ushort)0;
        }

        public static string GetKeyName(ushort vk)
        {
            foreach (var kvp in Map)
            {
                if (kvp.Value == vk) return kvp.Key;
            }
            return null;
        }
    }
}
