# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Mouse Recorder is a Windows desktop app (WPF, C#, .NET Framework 4.8) for automating mouse clicks and keyboard shortcuts via user-defined macros. Target environment is Windows 10/11 hospital computers where .NET cannot be installed (4.8 is pre-installed).

## Build & Run

Open `MouseRecorder.sln` in Visual Studio 2019 or 2022. NuGet restore pulls Newtonsoft.Json automatically.

- **Build**: `msbuild MouseRecorder.sln /p:Configuration=Release`
- **Output**: `MouseRecorder\bin\Release\MouseRecorder.exe`
- **Installer**: MSI packaging (not yet configured — use WiX or VS Installer project)

## Architecture

```
MouseRecorder/
├── Models/          Macro, MacroStep (POCO + INotifyPropertyChanged, JSON-serialized)
├── Native/          Win32 P/Invoke (mouse_event, SendInput, RegisterHotKey, GetCursorPos)
│                    KeyMapper (key name ↔ virtual key code dictionary)
├── Services/
│   ├── HotkeyService    Global hotkey registration via Win32 RegisterHotKey + WndProc hook
│   ├── MacroPlayer      Executes macro steps on background thread with cancellation
│   └── MacroStore       JSON persistence to %AppData%/MouseRecorder/macros.json
├── ViewModels/      MainViewModel (macro/step CRUD, playback state, hotkey management)
├── Views/
│   ├── MainWindow       Main UI — macro list + editor + step list + detail panel
│   ├── OverlayWindow    Full-screen semi-transparent crosshair for position picking
│   └── KeyCaptureWindow Modal dialog that captures keyboard combinations
└── Converters/      StringToVisibilityConverter for hotkey badge display
```

## Key Design Decisions

- **Absolute screen coordinates** for mouse clicks (single-monitor only)
- **Global hotkeys** via `RegisterHotKey` Win32 API — F12 is reserved as emergency stop, cannot be assigned to macros
- **No concurrent macros** — new hotkey presses ignored while a macro is running
- **Left-click only** — no right-click, double-click, or drag
- **Auto-save** — macros persist to JSON on every change
- **System tray** — minimize to tray on close/minimize, hotkeys remain active in background
- **ShutdownMode=OnExplicitShutdown** — app only exits via tray menu "Exit"

## Adding a New Step Type

1. Add value to `StepType` enum in `Models/MacroStep.cs`
2. Add properties to `MacroStep` if needed
3. Update `MacroStep.DisplayText` getter
4. Add execution logic in `MacroPlayer.ExecuteStep()`
5. Add "Add" button + editor panel in `MainWindow.xaml`
6. Add corresponding click handler in `MainWindow.xaml.cs` and ViewModel method

## NuGet Dependencies

- `Newtonsoft.Json 13.0.3` — JSON serialization (only external dependency)
