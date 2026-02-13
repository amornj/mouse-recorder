using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MouseRecorder.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StepType
    {
        LeftClick,
        KeyboardShortcut,
        Wait
    }

    public class MacroStep : INotifyPropertyChanged
    {
        private StepType _type;
        private int _x;
        private int _y;
        private List<string> _keys = new List<string>();
        private int _delayMs = 500;

        public StepType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        public int X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        public int Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        public List<string> Keys
        {
            get => _keys;
            set { _keys = value ?? new List<string>(); OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        public int DelayMs
        {
            get => _delayMs;
            set { _delayMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        [JsonIgnore]
        public string DisplayText
        {
            get
            {
                switch (Type)
                {
                    case StepType.LeftClick:
                        return $"Left Click at ({X}, {Y})";
                    case StepType.KeyboardShortcut:
                        return Keys.Count > 0 ? $"Key: {string.Join("+", Keys)}" : "Key: (none)";
                    case StepType.Wait:
                        return $"Wait {DelayMs} ms";
                    default:
                        return "Unknown";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
