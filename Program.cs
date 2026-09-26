// SPDX-License-Identifier: MIT

using System.Reflection;
using System.Runtime.InteropServices;

internal static class Program
{
  /// <summary>
  /// The main entry point for the application.
  /// </summary>
  [STAThread]
  static void Main()
  {
    TryEnablePerMonitorV2Dpi();
    ApplicationConfiguration.Initialize();
    SettingsManager.Load();
    LocalizationManager.Initialize(SettingsManager.CurrentAppSettings.Language);
    using var context = new HotkeyAppContext();
    Application.Run(context);
  }

  static void TryEnablePerMonitorV2Dpi()
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
  private readonly string _title = "havCurCtr";
  private readonly string _version = "1.1.0.0";
  private static string CurrentHotkey => HotkeySettings.Format(
      SettingsManager.CurrentAppSettings.HotkeyModifiers, SettingsManager.CurrentAppSettings.HotkeyKey);

  private readonly MessageWindow _messageWindow;
  private readonly NotifyIcon? _trayIcon;
  private const int HotkeyId = 1;
  private const uint MOD_NOREPEAT = 0x4000;
  private bool _hotkeyRegistered;
  private bool _editingHotkey;
  private bool _exiting;
  private int _registrationError;

  public HotkeyAppContext()
  {
    _messageWindow = new MessageWindow();
    _messageWindow.HotkeyPressed += OnHotkeyPressed;

    _trayIcon = new NotifyIcon
    {
      Text = "havCurCtr",
      Icon = LoadEmbeddedIconOrDefault("havCurCtr.ico"),
      Visible = true,
      ContextMenuStrip = BuildMenu()
    };

    UpdateTrayText();

    var settings = SettingsManager.CurrentAppSettings;
    if (!TryRegisterHotkey(settings.HotkeyModifiers, settings.HotkeyKey))
    {
      ShowRegistrationError();
    }
  }

  private ContextMenuStrip BuildMenu()
  {
    var menu = new ContextMenuStrip();
    var about = new ToolStripMenuItem(LocalizationManager.Get("About"), null, (_, __) =>
        MessageBox.Show($"{_title}\n" +
            $"{LocalizationManager.Get("AboutVersion")} {_version}\n" +
            $"Copyright © 2025-2026 René Nicolaus\n\n" +
            $"{LocalizationManager.Get("AboutDescription")}\n\n" +
            $"{LocalizationManager.Get("AboutHotkey")}: {CurrentHotkey}",
            LocalizationManager.Get("About"), MessageBoxButtons.OK, MessageBoxIcon.Information));
    var exit = new ToolStripMenuItem(LocalizationManager.Get("Exit"), null, (_, __) => ExitThread());
    menu.Items.Add(LocalizationManager.Get("ChangeHotkey"), null, (_, __) => ShowHotkeySettings());
    menu.Items.Add(LanguageMenu.Build(ChangeLanguage));
    menu.Items.Add(new ToolStripSeparator());
    menu.Items.Add(about);
    menu.Items.Add(new ToolStripSeparator());
    menu.Items.Add(exit);

    return menu;
  }

  private void ChangeLanguage(string? language)
  {
    if (_editingHotkey || _exiting || SettingsManager.CurrentAppSettings.Language == language)
    {
      return;
    }

    var updated = SettingsManager.CurrentAppSettings.Copy();
    updated.Language = language;
    if (!SettingsManager.TrySave(updated, out var error))
    {
      MessageBox.Show(LocalizationManager.Get("LanguageSaveFailed", error), _title,
          MessageBoxButtons.OK, MessageBoxIcon.Warning);

      return;
    }

    LocalizationManager.Initialize(language);
    var oldMenu = _trayIcon!.ContextMenuStrip;
    oldMenu?.Close();
    _trayIcon.ContextMenuStrip = BuildMenu();
    oldMenu?.Dispose();
    UpdateTrayText();
  }

  private void ShowHotkeySettings()
  {
    if (_editingHotkey || _exiting)
    {
      return;
    }

    _editingHotkey = true;
    UnregisterCurrentHotkey();

    if (_trayIcon?.ContextMenuStrip is { } menu)
    {
      menu.Enabled = false;
    }

    try
    {
      var settings = SettingsManager.CurrentAppSettings;
      using var dialog = new HotkeyDialog(settings.HotkeyModifiers, settings.HotkeyKey, TryApplyHotkey);
      dialog.ShowDialog();
    }
    finally
    {
      _editingHotkey = false;

      if (!_exiting)
      {
        if (_trayIcon?.ContextMenuStrip is { } currentMenu)
        {
          currentMenu.Enabled = true;
        }

        if (!_hotkeyRegistered)
        {
          var settings = SettingsManager.CurrentAppSettings;
          if (!TryRegisterHotkey(settings.HotkeyModifiers, settings.HotkeyKey))
          {
            ShowRegistrationError();
          }
        }
      }
    }
  }

  private bool TryApplyHotkey(uint modifiers, int key)
  {
    if (!TryRegisterHotkey(modifiers, key))
    {
      MessageBox.Show(Form.ActiveForm, LocalizationManager.Get("HotkeyChangeFailed", _registrationError),
          _title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

      return false;
    }

    var updated = SettingsManager.CurrentAppSettings.Copy();
    updated.HotkeyModifiers = modifiers;
    updated.HotkeyKey = key;
    if (!SettingsManager.TrySave(updated, out var error))
    {
      UnregisterCurrentHotkey();

      MessageBox.Show(Form.ActiveForm, LocalizationManager.Get("SettingsSaveFailed", error),
          _title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

      return false;
    }

    UpdateTrayText();

    return true;
  }

  private bool TryRegisterHotkey(uint modifiers, int key)
  {
    _hotkeyRegistered = NativeMethods.RegisterHotKey(_messageWindow.Handle, HotkeyId,
        modifiers | MOD_NOREPEAT, (uint)key);
    _registrationError = _hotkeyRegistered ? 0 : Marshal.GetLastWin32Error();

    return _hotkeyRegistered;
  }

  private void UnregisterCurrentHotkey()
  {
    if (_hotkeyRegistered)
    {
      NativeMethods.UnregisterHotKey(_messageWindow.Handle, HotkeyId);
      _hotkeyRegistered = false;
    }
  }

  private void ShowRegistrationError()
  {
    MessageBox.Show(LocalizationManager.Get("HotkeyRegistrationFailed", _registrationError),
        _title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
  }

  private void UpdateTrayText()
  {
    var tooltip = $"havCurCtr | {CurrentHotkey}";
    _trayIcon?.Text = tooltip.Length > 63 ? tooltip[..63] : tooltip;
  }

  protected override void ExitThreadCore()
  {
    Cleanup();

    base.ExitThreadCore();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      Cleanup();
    }

    base.Dispose(disposing);
  }

  private void Cleanup()
  {
    if (_exiting)
    {
      return;
    }

    _exiting = true;
    UnregisterCurrentHotkey();
    _messageWindow.Dispose();

    if (_trayIcon != null)
    {
      _trayIcon.Visible = false;
      _trayIcon.ContextMenuStrip?.Dispose();
      _trayIcon.Dispose();
    }
  }

  private void OnHotkeyPressed(object? sender, EventArgs e)
  {
    if (!_hotkeyRegistered || _editingHotkey || _exiting)
    {
      return;
    }

    try
    {
      var rect = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;
      int x = rect.Left + rect.Width / 2;
      int y = rect.Top + rect.Height / 2;
      NativeMethods.SetCursorPos(x, y);
    }
    catch (Exception ex)
    {
      MessageBox.Show(LocalizationManager.Get("CursorCenterFailed", ex.Message),
          _title, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
      if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
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
