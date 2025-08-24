// havCurCtr
//
// ABOUT
//
// A WinForms-based background app that registers a global hotkey and centers the mouse cursor on the primary monitor when pressed.
//
// REVISION HISTORY
//
// v1.0 (2025-08-24) - First release.
//
// LICENSE
//
// MIT License
//
// Copyright (c) 2025 René Nicolaus
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Reflection;
using System.Runtime.InteropServices;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        TryEnablePerMonitorV2Dpi();
        ApplicationConfiguration.Initialize();
        Application.Run(new HotkeyAppContext());
    }

    private static void TryEnablePerMonitorV2Dpi()
    {
        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        }
        catch
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
        }
    }
}

internal static partial class NativeMethods
{
    private const string _user32 = "user32.dll";

    [LibraryImport(_user32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport(_user32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    [LibraryImport(_user32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetCursorPos(int X, int Y);
}

/// <summary>
/// ApplicationContext that registers a global hotkey and listens on a hidden message-only window.
/// </summary>
internal sealed class HotkeyAppContext : ApplicationContext
{
    private readonly string _title = "havCurCtr by René \"Havoc\" Nicolaus";
    private readonly string _version = "1.0";
    private readonly string _hotkey = "Win + Shift + C";

    private readonly MessageWindow _messageWindow;
    private readonly NotifyIcon? _trayIcon;
    private readonly int _hotkeyId = 1;

    // Hotkey: Win + Shift + C
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    private const uint HOTKEY_MODIFIERS = MOD_WIN | MOD_SHIFT; // Win + Shift
    private const Keys HOTKEY_VK = Keys.C; // C = Center

    public HotkeyAppContext()
    {
        _messageWindow = new MessageWindow();
        _messageWindow.HotkeyPressed += OnHotkeyPressed;

        if (!NativeMethods.RegisterHotKey(_messageWindow.Handle, _hotkeyId, HOTKEY_MODIFIERS, (uint)HOTKEY_VK))
        {
            var error = Marshal.GetLastWin32Error();
            MessageBox.Show($"Failed to register hotkey (Error: {error}). Another app may be using it.", _title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            ExitThread();
            return;
        }

        _trayIcon = new NotifyIcon
        {
            Text = $"{_title} - {_hotkey}",
            Icon = LoadEmbeddedIconOrDefault("havCurCtr.ico"),
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        var about = new ToolStripMenuItem("About", null, (_, __) => MessageBox.Show($"Version {_version}\n\nCenters the mouse cursor on the primary monitor.\n\nHotkey: {_hotkey}", _title, MessageBoxButtons.OK, MessageBoxIcon.Information));
        var exit = new ToolStripMenuItem("Exit", null, (_, __) => ExitThread());
        menu.Items.Add(about);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exit);
        return menu;
    }

    protected override void ExitThreadCore()
    {
        try
        {
            NativeMethods.UnregisterHotKey(_messageWindow.Handle, _hotkeyId);
        }
        catch
        {
            // Ignore
        }
        _messageWindow?.Dispose();
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        base.ExitThreadCore();
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        try
        {
            var rect = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;
            int x = rect.Left + rect.Width / 2;
            int y = rect.Top + rect.Height / 2;
            NativeMethods.SetCursorPos(x, y);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to center mouse cursor: {ex.Message}", _title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Icon LoadEmbeddedIconOrDefault(string fileName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = Array.Find(assembly.GetManifestResourceNames(), x => x.EndsWith($".{fileName}", StringComparison.OrdinalIgnoreCase));
            if (resourceName is null)
            {
                return SystemIcons.Application;
            }
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return SystemIcons.Application;
            }
            return new Icon(stream);
        }
        catch
        {
            return SystemIcons.Application;
        }
    }

    /// <summary>
    /// Hidden message-only window to receive WM_HOTKEY without showing UI.
    /// </summary>
    private sealed class MessageWindow : NativeWindow, IDisposable
    {
        public event EventHandler? HotkeyPressed;

        private static readonly IntPtr HWND_MESSAGE = new(-3);

        private const int WM_HOTKEY = 0x0312;

        public MessageWindow()
        {
            var cp = new CreateParams
            {
                Caption = "havCurCtrMessageWindow",
                Parent = HWND_MESSAGE
            };
            CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
            GC.SuppressFinalize(this);
        }
    }
}
