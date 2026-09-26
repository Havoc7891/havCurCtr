=================
    havCurCtr

 VERSION 1.1.0.0
=================

Copyright (c) 2025-2026 René Nicolaus

Build: Windows (.NET 10, WinForms, Per-Monitor-V2 DPI-aware)
Default hotkey: Ctrl + Alt + C (Customizable from the tray menu)
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
- .NET 10 Desktop Runtime (x64)

If you don't already have the .NET 10 Desktop Runtime, download and install from: https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime

=============
3. How to run
=============
1) Extract the ZIP anywhere (e.g., C:\Apps\havCurCtr).
2) Run havCurCtr.exe (No admin required).
3) Find the tray icon and right-click it to open the settings menu. Use the default global hotkey (Ctrl + Alt + C) to center the cursor.

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
Find the tray icon and right-click it to open the settings menu.

Menu items:
- Change hotkey...: opens the dialog for changing the global shortcut for centering the cursor
- Language: switches UI language immediately and saves the preference
 - System default: matches the OS language and falls back to English if unsupported
- About: shows information about the app and the current hotkey
- Exit: quits the app

=========================
6. Customizing the hotkey
=========================
The default global hotkey is Ctrl + Alt + C. To change it:

1) Right-click the tray icon and choose 'Change hotkey...'.
2) Click Change, press the key you want, and select the Ctrl, Alt, Shift, or Win modifiers.
3) Click OK to apply the shortcut immediately and save it. No restart is required.

Reset to defaults selects Ctrl + Alt + C; click OK to apply it. Cancel leaves the current shortcut unchanged.

The shortcut is saved as HotkeyModifiers and HotkeyKey in %AppData%\havCurCtr\config.json and restored the next time the app starts.

If another app already uses the shortcut, an error lets you choose another combination. If the saved shortcut is unavailable at startup, the app remains accessible through the tray so you can change it.

========
7. Notes
========
- Uses a global system-wide hotkey; if the selected shortcut is taken, the app shows an error and remains accessible through the tray.
- No elevation required. Works in the user session.

============
8. Uninstall
============
- Exit from the tray -> delete the app folder
- Delete config file in %AppData%\havCurCtr (Win + R -> %AppData% -> Enter)
- If you enabled auto-start: remove the shortcut from shell:startup or delete the 'havCurCtr' value from:
  HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run

==================
9. Troubleshooting
==================
- Hotkey doesn't work: Another app may own the combo. Right-click the tray icon -> Change hotkey... -> choose another combination -> OK.
- No tray icon: Make sure Windows hasn't hidden it; expand the tray overflow.
- Multi-monitor: Centers on the primary monitor by design.

=============
10. Changelog
=============

Version 1.1.0.0 - 2026-09-26
- Upgraded to .NET 10.
- Added customizable hotkey settings to the tray menu.
- Changed the default global hotkey to Ctrl + Alt + C.
- Added English and German translations.

Version 1.0.0.0 - 2025-08-24
- First release.

===========
11. License
===========
Licensed under MIT - see LICENSE.txt

===========
End of file
===========