// SPDX-License-Identifier: MIT

using System.Text.Json;

internal static class SettingsManager
{
  private static readonly string _folder = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "havCurCtr");
  private static readonly string _path = Path.Combine(_folder, "config.json");

  public static AppSettings CurrentAppSettings { get; private set; } = new();

  public static void Load()
  {
    try
    {
      var settings = File.Exists(_path)
          ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path)) ?? new AppSettings()
          : new AppSettings();

      if (!HotkeySettings.IsValid(settings.HotkeyModifiers, settings.HotkeyKey))
      {
        settings.HotkeyModifiers = HotkeySettings.DefaultModifiers;
        settings.HotkeyKey = HotkeySettings.DefaultKey;
      }

      CurrentAppSettings = settings;
    }
    catch
    {
      // Missing or invalid settings must not prevent startup
      CurrentAppSettings = new AppSettings();
    }
  }

  public static void Save()
  {
    TrySave(CurrentAppSettings, out _);
  }

  public static bool TrySave(AppSettings settings, out string? error)
  {
    try
    {
      Directory.CreateDirectory(_folder);
      var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
      var temporaryPath = _path + ".tmp";
      File.WriteAllText(temporaryPath, json);
      File.Move(temporaryPath, _path, overwrite: true);
      CurrentAppSettings = settings;
      error = null;
      return true;
    }
    catch (Exception ex)
    {
      error = ex.Message;
      return false;
    }
  }
}
