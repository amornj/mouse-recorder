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
        LeftDoubleClick,
        RightClick,
        KeyboardShortcut,
        Keystroke,
        TypeText,
        Wait
    }

    public class MacroStep : INotifyPropertyChanged
    {
        private StepType _type;
        private int _x;
        private int _y;
        private List<string> _keys = new List<string>();
        private int _delayMs = 500;
        private string _text = "";
        private string _annotation = "";
        private bool _isActive;

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

        public string Text
        {
            get => _text;
            set { _text = value ?? ""; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        public string Annotation
        {
            get => _annotation;
            set { _annotation = value ?? ""; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        [JsonIgnore]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public string DisplayText
        {
            get
            {
                string label;
                switch (Type)
                {
                    case StepType.LeftClick:
                        label = $"Left Click at ({X}, {Y})"; break;
                    case StepType.LeftDoubleClick:
                        label = $"Double Click at ({X}, {Y})"; break;
                    case StepType.RightClick:
                        label = $"Right Click at ({X}, {Y})"; break;
                    case StepType.KeyboardShortcut:
                        label = Keys.Count > 0 ? $"Key: {string.Join("+", Keys)}" : "Key: (none)"; break;
                    case StepType.Keystroke:
                        label = Keys.Count > 0 ? $"Keystroke: {Keys[0]}" : "Keystroke: (none)"; break;
                    case StepType.TypeText:
                        var preview = Text.Length > 30 ? Text.Substring(0, 30) + "..." : Text;
                        label = $"Type: {preview}"; break;
                    case StepType.Wait:
                        label = $"Wait {DelayMs} ms"; break;
                    default:
                        label = "Unknown"; break;
                }
                return label;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
