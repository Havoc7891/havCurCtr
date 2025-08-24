# havCurCtr

A WinForms-based background app that registers a global hotkey and centers the mouse cursor on the primary monitor when pressed.

## Table of Contents

- [Requirements](#requirements)
- [How to run](#how-to-run)
- [Start with Windows (Optional)](#start-with-windows-optional)
- [Customizing the hotkey](#customizing-the-hotkey)
- [Contributing](#contributing)
- [License](#license)

## Requirements
- Windows 10 or later
- [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime)

## How to run

1) Extract the [ZIP](https://github.com/Havoc7891/havCurCtr/releases/download/v1.0/havCurCtr-1.0-win-x64.zip) anywhere (e.g., `C:\Apps\havCurCtr`).
2) Run havCurCtr.exe (no admin required).
3) Look for the tray icon. Use Win + Shift + C to center the cursor.

![Screenshot](/screenshot/havCurCtr.png)

## Start with Windows (Optional)
Option A - Startup folder (Per-user)
- Press Win + R -> type: `shell:startup` -> OK
- Place a shortcut to havCurCtr.exe in that folder

Option B - Registry (Per-user)
Create a file named 'havCurCtr-Startup.reg' with the content below, edit the path, then double-click to add:

```reg
Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run]
"havCurCtr"="\"C:\\Apps\\havCurCtr\\havCurCtr.exe\""
```

## Customizing the hotkey
The hotkey is compiled in. To change it:
- Edit Program.cs -> HOTKEY_MODIFIERS / HOTKEY_VK
- Rebuild (Release): `dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true`

## Contributing

Feel free to suggest features or report issues. However, please note that pull requests will not be accepted.

## License

Copyright &copy; 2025 Ren&eacute; Nicolaus

Released under the [MIT license](/LICENSE).