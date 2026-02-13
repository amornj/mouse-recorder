# Mouse Recorder

A Windows desktop application for automating mouse clicks and keyboard shortcuts via user-defined macros.

Built with C# / WPF / .NET Framework 4.8 — runs on Windows 10 and 11 with no additional runtime installation.

## Features

- **Macro editor** — create sequences of left-clicks, keyboard shortcuts, and wait delays
- **Position picker** — fullscreen crosshair overlay to capture exact screen coordinates
- **Keyboard shortcut capture** — press any key combination to record it as a step
- **Global hotkeys** — assign macros to F1–F11 or Ctrl+key combos, trigger from any application
- **Emergency stop** — press F12 to immediately halt a running macro
- **Loop / repeat** — run a macro N times or infinitely (set repeat to 0)
- **Import / export** — share macros as JSON files
- **System tray** — minimize to tray, hotkeys remain active in the background
- **Auto-save** — macros persist automatically to `%AppData%/MouseRecorder/macros.json`

## Screenshot

```
┌──────────────────────────────────────────────────────────────┐
│ [+ New Macro] [Delete] [▶ Play] [■ Stop] | [Import] [Export]│
├────────────────┬─────────────────────────────────────────────┤
│ Macros         │ Name:   [My Macro          ]               │
│                │ Hotkey: [F6 ▼]                              │
│ [F6] My Macro  │ Repeat: [1] times (0 = loop forever)       │
│ [F7] Another   │                                            │
│                │ Steps:                                      │
│                │  1. Left Click at (500, 300)                │
│                │  2. Wait 1000 ms                            │
│                │  3. Key: Ctrl+C                             │
│                │                                            │
│                │ [+ Click] [+ Keyboard] [+ Wait]            │
│                │ [▲ Up] [▼ Down] [✖ Delete]                  │
├────────────────┴─────────────────────────────────────────────┤
│ Ready                                    F12 = Stop Macro    │
└──────────────────────────────────────────────────────────────┘
```

## Requirements

- **To run**: Windows 10 or 11 (.NET Framework 4.8 is pre-installed)
- **To build**: Visual Studio 2019 or 2022 with ".NET desktop development" workload

## Build

1. Clone the repository
2. Open `MouseRecorder.sln` in Visual Studio
3. NuGet restore will pull `Newtonsoft.Json` automatically
4. Build with `F5` (Debug) or `Ctrl+Shift+B`

Or from command line:
```
msbuild MouseRecorder.sln /p:Configuration=Release
```

Output: `MouseRecorder\bin\Release\MouseRecorder.exe`

## Macro JSON Format

Macros are stored as JSON and can be imported/exported:

```json
[
  {
    "Name": "My Macro",
    "Hotkey": "F6",
    "RepeatCount": 1,
    "Steps": [
      { "Type": "LeftClick", "X": 500, "Y": 300 },
      { "Type": "Wait", "DelayMs": 1000 },
      { "Type": "KeyboardShortcut", "Keys": ["Ctrl", "C"] }
    ]
  }
]
```

## License

MIT
