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
- **To build**: Visual Studio 2019/2022 Build Tools + [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)

## Build

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" MouseRecorder.sln -p:Configuration=Release -restore
```

Output: `MouseRecorder\bin\Release\MouseRecorder.exe`

## Deploy to Another Computer

No installer needed. Just copy these files from `MouseRecorder\bin\Release\`:

- `MouseRecorder.exe`
- `Newtonsoft.Json.dll`

Put them in the same folder on the target machine and run the exe.

## Usage

1. Click **New Macro** to create a macro
2. Add steps: **+ Click**, **+ Keyboard**, or **+ Wait**
3. For click steps, use **Pick** to select screen coordinates via crosshair overlay
4. Assign a **Hotkey** to trigger the macro globally
5. Set **Repeat** count (0 = loop forever)
6. Press the hotkey from any app to run the macro
7. Press **F12** to emergency stop

The app minimizes to the system tray. Right-click the tray icon and choose **Exit** to close.

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
