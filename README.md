# Mouse Recorder

A Windows desktop application for automating mouse clicks and keyboard shortcuts via user-defined macros.

Built with C# / WPF / .NET Framework 4.8 — runs on Windows 10 and 11 with no additional runtime installation.

## Features

- **Macro editor** — create sequences of clicks, keyboard actions, and wait delays
- **Click types** — left click, double click, and right click with crosshair position picker
- **Keyboard shortcuts** — capture key combinations like Ctrl+C via a capture dialog
- **Keystroke** — single key press from a dropdown (Tab, PgUp, Home, F1–F12, arrows, etc.) for compact keyboards
- **Type Text** — types a preconfigured text string character-by-character via Unicode input
- **Step annotations** — double-click the Note column to add a reminder for each step
- **Playback highlight** — active step is highlighted during macro execution
- **Global hotkeys** — assign macros to F1–F11 or Ctrl+key combos, trigger from any application
- **Emergency stop** — press F12 to immediately halt a running macro
- **Loop / repeat** — run a macro N times or infinitely (set repeat to 0)
- **Responsive layout** — step list dynamically expands to fill window height
- **Import / export** — share macros as JSON files
- **System tray** — minimize to tray, hotkeys remain active in the background
- **Auto-save** — macros persist automatically to `%AppData%/MouseRecorder/macros.json`

## Screenshot

```
┌───────────────────────────────────────────────────────────────────┐
│ [New Macro] [Delete] [▶ Play] [■ Stop]  [Import] [Export]        │
├───────────────┬───────────────────────────────────────────────────┤
│ Macros        │ Name:   [My Macro          ]                     │
│               │ Hotkey: [F6 ▼]                                   │
│ [F6] My Macro │ Repeat: [1] times (0 = loop forever)             │
│ [F7] Another  │                                                  │
│               │ Steps                       Note                 │
│               │ ┌───────────────────────┬────────────────────┐   │
│               │ │ Left Click at (500,300)│ open patient chart │   │
│               │ │ Wait 1000 ms          │                    │   │
│               │ │ Key: Ctrl+A           │ select all text    │   │
│               │ │ Keystroke: Home       │ go to start        │   │
│               │ │ Type: patient name    │                    │   │
│               │ │ Wait 500 ms           │                    │   │
│               │ └───────────────────────┴────────────────────┘   │
│               │ [+Click][+Dbl][+Right][+Shortcut][+Keystroke]    │
│               │ [+Type Text][+Wait] | [▲ Up][▼ Down][✖ Delete]   │
├───────────────┴───────────────────────────────────────────────────┤
│ Ready                                         F12 = Stop Macro   │
└───────────────────────────────────────────────────────────────────┘
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
2. Add steps using the buttons:
   - **+ Click / + Dbl Click / + Right Click** — pick a screen position via crosshair overlay
   - **+ Shortcut** — capture a key combination (e.g. Ctrl+C)
   - **+ Keystroke** — select a single key from a dropdown (Home, PgUp, F1, etc.)
   - **+ Type Text** — type a text string character-by-character
   - **+ Wait** — pause for a specified duration
3. Double-click the **Note** column to annotate any step
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
      { "Type": "LeftClick", "X": 500, "Y": 300, "Annotation": "open chart" },
      { "Type": "LeftDoubleClick", "X": 200, "Y": 150 },
      { "Type": "RightClick", "X": 300, "Y": 200 },
      { "Type": "Wait", "DelayMs": 1000 },
      { "Type": "KeyboardShortcut", "Keys": ["Ctrl", "C"] },
      { "Type": "Keystroke", "Keys": ["Home"] },
      { "Type": "TypeText", "Text": "patient name" }
    ]
  }
]
```

## License

MIT
