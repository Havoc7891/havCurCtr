# havCurCtr

A WinForms-based background app that registers a global hotkey and centers the mouse cursor on the primary monitor when pressed.

## Table of Contents

- [Requirements](#requirements)
- [How to run](#how-to-run)
- [Start with Windows (Optional)](#start-with-windows-optional)
- [Tray menu](#tray-menu)
- [Customizing the hotkey](#customizing-the-hotkey)
- [Localization](#localization)
- [Building from source](#building-from-source)
- [Contributing](#contributing)
- [License](#license)

## Requirements

- Windows 10 or later
- [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## How to run

1) Extract the [ZIP](https://github.com/Havoc7891/havCurCtr/releases/download/v1.1.0.0/havCurCtr-1.1.0.0-win-x64.zip) anywhere (e.g., `C:\Apps\havCurCtr`).
2) Run `havCurCtr.exe` (no admin required).
3) Find the tray icon and right-click it to open the settings menu. Use the default global hotkey (`Ctrl + Alt + C`) to center the cursor.

![Screenshot](/screenshot/havCurCtr.png)

## Start with Windows (Optional)

Option A - Startup folder (Per-user)

- Press Win + R -> type: `shell:startup` -> OK
- Place a shortcut to `havCurCtr.exe` in that folder

Option B - Registry (Per-user)

Create a file named `havCurCtr-Startup.reg` with the content below, edit the path, then double-click to add:

```reg
Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run]
"havCurCtr"="\"C:\\Apps\\havCurCtr\\havCurCtr.exe\""
```

## Tray menu

Find the tray icon and right-click it to open the settings menu.

Menu items:

- **Change hotkey...**: opens the dialog for changing the global shortcut for centering the cursor
- **Language**: switches UI language immediately and saves the preference
  - **System default**: matches the OS language and falls back to English if unsupported
- **About**: shows information about the app and the current hotkey
- **Exit**: quits the app

## Customizing the hotkey

The default global hotkey is `Ctrl + Alt + C`. To change it:

1) Right-click the tray icon and choose **Change hotkey...**.
2) Click **Change**, press the key you want, and select the **Ctrl**, **Alt**, **Shift**, or **Win** modifiers.
3) Click **OK** to apply the shortcut immediately and save it. No restart is required.

**Reset to defaults** selects `Ctrl + Alt + C`; click **OK** to apply it. **Cancel** leaves the current shortcut unchanged.

The shortcut is saved as `HotkeyModifiers` and `HotkeyKey` in `%AppData%\havCurCtr\config.json` and restored the next time the app starts.

If another app already uses the shortcut, an error lets you choose another combination. If the saved shortcut is unavailable at startup, the app remains accessible through the tray so you can change it.

## Localization

The app currently supports the following languages:

- **English (en)**
- **German (de)**

If you would like to contribute translations for additional languages, please submit a pull request.

The translation files are in the `languages` folder; use `languages/en.json` as the template.

## Building from source

Install the .NET 10 SDK (x64) on Windows 10 or later. Clone or download this repository, then open a terminal in its root folder.

Build the app:

```powershell
dotnet build havCurCtr.csproj -c Release
```

Run `bin\Release\net10.0-windows\havCurCtr.exe`.

To publish a Windows x64 build for distribution:

```powershell
dotnet publish havCurCtr.csproj -c Release -r win-x64 --self-contained false -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false
```

The output is in `bin\Release\net10.0-windows\win-x64\publish`. Distribute the entire folder, including `languages`. This build requires the .NET 10 Desktop Runtime (x64) on the target machine.

## Contributing

Thank you for your interest! Suggestions for features and bug reports are always welcome via issues.

To maintain a consistent design and quality for this project, changes are implemented by the maintainer rather than via direct pull requests, except for localization updates.

## License

Copyright &copy; 2025-2026 Ren&eacute; Nicolaus

Released under the [MIT license](/LICENSE).
