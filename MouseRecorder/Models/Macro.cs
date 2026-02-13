using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace MouseRecorder.Models
{
    public class Macro : INotifyPropertyChanged
    {
        private string _name = "New Macro";
        private string _hotkey = "";
        private int _repeatCount = 1;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Hotkey
        {
            get => _hotkey;
            set { _hotkey = value; OnPropertyChanged(); }
        }

        public int RepeatCount
        {
            get => _repeatCount;
            set { _repeatCount = value; OnPropertyChanged(); }
        }

        public List<MacroStep> Steps { get; set; } = new List<MacroStep>();

        [JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
