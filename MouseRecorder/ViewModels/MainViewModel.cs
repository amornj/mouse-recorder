using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MouseRecorder.Models;
using MouseRecorder.Native;
using MouseRecorder.Services;

namespace MouseRecorder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MacroStore _store;
        private readonly MacroPlayer _player;
        private HotkeyService _hotkeyService;

        public ObservableCollection<Macro> Macros { get; } = new ObservableCollection<Macro>();
        public ObservableCollection<MacroStep> Steps { get; } = new ObservableCollection<MacroStep>();
        public List<string> AvailableHotkeys => HotkeyService.AvailableHotkeys;
        public IList<string> CommonKeystrokes => KeyMapper.CommonKeystrokes;

        private Macro _selectedMacro;
        public Macro SelectedMacro
        {
            get => _selectedMacro;
            set
            {
                if (_selectedMacro == value) return;
                SyncStepsToMacro();
                _selectedMacro = value;
                LoadMacroIntoEditor();
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedMacro));
                OnPropertyChanged(nameof(SelectedMacroHotkey));
            }
        }

        public bool HasSelectedMacro => _selectedMacro != null;

        public string SelectedMacroHotkey
        {
            get => _selectedMacro?.Hotkey ?? "(None)";
            set
            {
                if (_selectedMacro == null) return;
                var oldHotkey = _selectedMacro.Hotkey;
                _selectedMacro.Hotkey = value == "(None)" ? "" : value;
                OnPropertyChanged();
                ReregisterHotkey(_selectedMacro, oldHotkey);
                AutoSave();
            }
        }

        private MacroStep _selectedStep;
        public MacroStep SelectedStep
        {
            get => _selectedStep;
            set
            {
                _selectedStep = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedStep));
                OnPropertyChanged(nameof(IsClickStepSelected));
                OnPropertyChanged(nameof(IsWaitStepSelected));
                OnPropertyChanged(nameof(IsKeyStepSelected));
                OnPropertyChanged(nameof(IsKeystrokeStepSelected));
                OnPropertyChanged(nameof(IsTextStepSelected));
            }
        }

        public bool HasSelectedStep => _selectedStep != null;
        public bool IsClickStepSelected => _selectedStep?.Type == StepType.LeftClick
            || _selectedStep?.Type == StepType.LeftDoubleClick
            || _selectedStep?.Type == StepType.RightClick;
        public bool IsWaitStepSelected => _selectedStep?.Type == StepType.Wait;
        public bool IsKeyStepSelected => _selectedStep?.Type == StepType.KeyboardShortcut;
        public bool IsKeystrokeStepSelected => _selectedStep?.Type == StepType.Keystroke;
        public bool IsTextStepSelected => _selectedStep?.Type == StepType.TypeText;

        private int _selectedStepIndex = -1;
        public int SelectedStepIndex
        {
            get => _selectedStepIndex;
            set { _selectedStepIndex = value; OnPropertyChanged(); }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotPlaying));
            }
        }
        public bool IsNotPlaying => !_isPlaying;

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            _store = new MacroStore();
            _player = new MacroPlayer();

            _player.StatusChanged += text =>
                Application.Current?.Dispatcher?.Invoke(() => StatusText = text);
            _player.PlaybackFinished += () =>
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    ClearActiveStep();
                    IsPlaying = false;
                });
            _player.StepExecuting += index =>
                Application.Current?.Dispatcher?.Invoke(() => SetActiveStep(index));

            LoadMacros();
        }

        public void SetHotkeyService(HotkeyService service)
        {
            _hotkeyService = service;
            _hotkeyService.MacroHotkeyPressed += OnMacroHotkeyPressed;
            _hotkeyService.StopHotkeyPressed += OnStopHotkeyPressed;
            RegisterAllHotkeys();
        }

        private void LoadMacros()
        {
            var macros = _store.Load();
            Macros.Clear();
            foreach (var m in macros)
                Macros.Add(m);

            if (Macros.Count > 0)
                SelectedMacro = Macros[0];
        }

        private void LoadMacroIntoEditor()
        {
            Steps.Clear();
            if (_selectedMacro == null) return;

            foreach (var step in _selectedMacro.Steps)
                Steps.Add(step);
        }

        private void SyncStepsToMacro()
        {
            if (_selectedMacro == null) return;
            _selectedMacro.Steps = Steps.ToList();
        }

        // ── Macro CRUD ─────────────────────────────────────────

        public void AddMacro()
        {
            var macro = new Macro { Name = $"Macro {Macros.Count + 1}" };
            Macros.Add(macro);
            SelectedMacro = macro;
            AutoSave();
        }

        public void DeleteMacro()
        {
            if (_selectedMacro == null) return;
            var toDelete = _selectedMacro;
            _hotkeyService?.UnregisterMacroHotkey(toDelete.Id);

            int idx = Macros.IndexOf(toDelete);
            Macros.Remove(toDelete);

            if (Macros.Count > 0)
                SelectedMacro = Macros[Math.Min(idx, Macros.Count - 1)];
            else
                SelectedMacro = null;

            AutoSave();
        }

        // ── Step CRUD ───────────────────────────────────────────

        public void AddClickStep(int x, int y)
        {
            if (_selectedMacro == null) return;
            var step = new MacroStep { Type = StepType.LeftClick, X = x, Y = y };
            Steps.Add(step);
            SelectedStep = step;
            SelectedStepIndex = Steps.Count - 1;
            AutoSave();
        }

        public void AddDoubleClickStep(int x, int y)
        {
            if (_selectedMacro == null) return;
            var step = new MacroStep { Type = StepType.LeftDoubleClick, X = x, Y = y };
            Steps.Add(step);
            SelectedStep = step;
            SelectedStepIndex = Steps.Count - 1;
            AutoSave();
        }

        public void AddRightClickStep(int x, int y)
        {
            if (_selectedMacro == null) return;
            var step = new MacroStep { Type = StepType.RightClick, X = x, Y = y };
            Steps.Add(step);
            SelectedStep = step;
            SelectedStepIndex = Steps.Count - 1;
            AutoSave();
        }

        public void AddKeyStep(List<string> keys)
        {
            if (_selectedMacro == null) return;
            var step = new MacroStep { Type = StepType.KeyboardShortcut, Keys = keys };
            Steps.Add(step);
            SelectedStep = step;
            SelectedStepIndex = Steps.Count - 1;
            AutoSave();
        }

        public void AddKeystrokeStep(string keyName)
        {
            if (_selectedMacro == null) return;
            var step = new MacroStep { Type = StepType.Keystroke, Keys = new List<string> { keyName } };
            Steps.Add(step);
            SelectedStep = step;
            SelectedStepIndex = Steps.Count - 1;
            AutoSave();
        }

        public void AddTextStep()
        {
            if (_selectedMacro == null) return;
            var step = new MacroStep { Type = StepType.TypeText, Text = "" };
            Steps.Add(step);
            SelectedStep = step;
            SelectedStepIndex = Steps.Count - 1;
            AutoSave();
        }

        public void AddWaitStep()
        {
            if (_selectedMacro == null) return;
            var step = new MacroStep { Type = StepType.Wait, DelayMs = 500 };
            Steps.Add(step);
            SelectedStep = step;
            SelectedStepIndex = Steps.Count - 1;
            AutoSave();
        }

        public void UpdateClickStepPosition(int x, int y)
        {
            if (_selectedStep == null || (_selectedStep.Type != StepType.LeftClick
                && _selectedStep.Type != StepType.LeftDoubleClick
                && _selectedStep.Type != StepType.RightClick)) return;
            _selectedStep.X = x;
            _selectedStep.Y = y;
            AutoSave();
        }

        public void UpdateKeyStep(List<string> keys)
        {
            if (_selectedStep == null || _selectedStep.Type != StepType.KeyboardShortcut) return;
            _selectedStep.Keys = keys;
            AutoSave();
        }

        public void UpdateKeystrokeStep(string keyName)
        {
            if (_selectedStep == null || _selectedStep.Type != StepType.Keystroke) return;
            _selectedStep.Keys = new List<string> { keyName };
            AutoSave();
        }

        public void DeleteSelectedStep()
        {
            if (_selectedStep == null) return;
            int idx = SelectedStepIndex;
            Steps.Remove(_selectedStep);

            if (Steps.Count > 0)
            {
                int newIdx = Math.Min(idx, Steps.Count - 1);
                SelectedStepIndex = newIdx;
                SelectedStep = Steps[newIdx];
            }
            else
            {
                SelectedStep = null;
            }
            AutoSave();
        }

        public void MoveStepUp()
        {
            int idx = SelectedStepIndex;
            if (idx <= 0) return;
            Steps.Move(idx, idx - 1);
            SelectedStepIndex = idx - 1;
            AutoSave();
        }

        public void MoveStepDown()
        {
            int idx = SelectedStepIndex;
            if (idx < 0 || idx >= Steps.Count - 1) return;
            Steps.Move(idx, idx + 1);
            SelectedStepIndex = idx + 1;
            AutoSave();
        }

        // ── Import / Export ─────────────────────────────────────

        public void ImportMacros(string path)
        {
            try
            {
                var imported = _store.ImportFromFile(path);
                foreach (var m in imported)
                {
                    m.Id = Guid.NewGuid().ToString();
                    Macros.Add(m);
                }
                AutoSave();
                RegisterAllHotkeys();
                StatusText = $"Imported {imported.Count} macro(s)";
            }
            catch (Exception ex)
            {
                StatusText = $"Import failed: {ex.Message}";
            }
        }

        public void ExportMacros(string path)
        {
            try
            {
                SyncStepsToMacro();
                _store.ExportToFile(Macros.ToList(), path);
                StatusText = $"Exported {Macros.Count} macro(s)";
            }
            catch (Exception ex)
            {
                StatusText = $"Export failed: {ex.Message}";
            }
        }

        // ── Playback ────────────────────────────────────────────

        private async void OnMacroHotkeyPressed(string macroId)
        {
            if (_player.IsPlaying) return;
            var macro = Macros.FirstOrDefault(m => m.Id == macroId);
            if (macro == null) return;

            SyncStepsToMacro();
            IsPlaying = true;
            await Task.Run(() => _player.PlayAsync(macro));
        }

        private void OnStopHotkeyPressed()
        {
            _player.Stop();
        }

        public async void PlaySelectedMacro()
        {
            if (_player.IsPlaying || _selectedMacro == null) return;
            SyncStepsToMacro();
            if (_selectedMacro.Steps.Count == 0)
            {
                StatusText = "Macro has no steps";
                return;
            }
            IsPlaying = true;
            await Task.Run(() => _player.PlayAsync(_selectedMacro));
        }

        public void StopMacro()
        {
            _player.Stop();
        }

        private void SetActiveStep(int index)
        {
            ClearActiveStep();
            if (index >= 0 && index < Steps.Count)
                Steps[index].IsActive = true;
        }

        private void ClearActiveStep()
        {
            foreach (var step in Steps)
                step.IsActive = false;
        }

        // ── Hotkey management ───────────────────────────────────

        private void RegisterAllHotkeys()
        {
            if (_hotkeyService == null) return;
            foreach (var macro in Macros)
            {
                if (!string.IsNullOrEmpty(macro.Hotkey))
                    _hotkeyService.RegisterMacroHotkey(macro.Id, macro.Hotkey);
            }
        }

        private void ReregisterHotkey(Macro macro, string oldHotkey)
        {
            if (_hotkeyService == null) return;
            _hotkeyService.UnregisterMacroHotkey(macro.Id);
            if (!string.IsNullOrEmpty(macro.Hotkey) && macro.Hotkey != "(None)")
            {
                if (!_hotkeyService.RegisterMacroHotkey(macro.Id, macro.Hotkey))
                {
                    StatusText = $"Failed to register hotkey {macro.Hotkey} — it may be in use";
                    macro.Hotkey = "";
                    OnPropertyChanged(nameof(SelectedMacroHotkey));
                }
            }
        }

        // ── Persistence ─────────────────────────────────────────

        public void AutoSave()
        {
            SyncStepsToMacro();
            _store.Save(Macros.ToList());
        }

        public void Cleanup()
        {
            AutoSave();
            _hotkeyService?.Dispose();
        }

        // ── INotifyPropertyChanged ──────────────────────────────

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
