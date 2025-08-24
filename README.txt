=============
  havCurCtr

 VERSION 1.0
=============

Copyright (c) 2025 René Nicolaus

Build: Windows (.NET 8, WinForms, Per-Monitor-V2 DPI-aware)
Hotkey: Win + Shift + C (Customizable in source code)
Source Code: https://github.com/Havoc7891/havCurCtr

========
Contents
========
1. What it does
2. Requirements
3. How to run
4. Start with Windows (Optional)
5. Tray menu
6. Customizing the hotkey
7. Notes
8. Uninstall
9. Troubleshooting
10. Changelog
11. License

===============
1. What it does
===============
A WinForms-based background app that registers a global hotkey and centers the mouse cursor on the primary monitor when pressed.

===============
2. Requirements
===============
- Windows 10 or later
- .NET 8 Desktop Runtime (x64)

If you don't already have the .NET 8 Desktop Runtime, download and install from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime

=============
3. How to run
=============
1) Extract the ZIP anywhere (e.g., C:\Apps\havCurCtr).
2) Run havCurCtr.exe (No admin required).
3) Look for the tray icon. Use Win + Shift + C to center the cursor.

================================
4. Start with Windows (Optional)
================================
Option A - Startup folder (Per-user)
- Press Win + R -> type: shell:startup -> OK
- Place a shortcut to havCurCtr.exe in that folder

Option B - Registry (Per-user)
Create a file named 'havCurCtr-Startup.reg' with the content below, edit the path, then double-click to add:

Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run]
"havCurCtr"="\"C:\\Apps\\havCurCtr\\havCurCtr.exe\""

============
5. Tray menu
============
Right-click the tray icon for About / Exit.

=========================
6. Customizing the hotkey
=========================
The hotkey is compiled in. To change it:
- Edit Program.cs -> HOTKEY_MODIFIERS / HOTKEY_VK
- Rebuild (Release): dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true

========
7. Notes
========
- Uses a global system-wide hotkey; if Win + Shift + C is taken, the app shows an error.
- No elevation required. Works in the user session.

============
8. Uninstall
============
- Exit from the tray -> delete the app folder.
- If you enabled auto-start: remove the shortcut from shell:startup or delete the 'havCurCtr' value from:
  HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run

==================
9. Troubleshooting
==================
- Hotkey doesn't work: Another app may own the combo. Pick a different hotkey and rebuild.
- No tray icon: Make sure Windows hasn't hidden it; expand the tray overflow.
- Multi-monitor: Centers on the primary monitor by design.

=============
10. Changelog
=============

Version 1.0 - 2025-08-24
- First release.

===========
11. License
===========
Licensed under MIT - see LICENSE.txt

===========
End of file
===========