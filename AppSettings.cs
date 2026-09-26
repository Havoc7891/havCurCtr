// SPDX-License-Identifier: MIT

internal sealed class AppSettings
{
  public uint HotkeyModifiers { get; set; } = HotkeySettings.DefaultModifiers;
  public int HotkeyKey { get; set; } = HotkeySettings.DefaultKey;
  public string? Language { get; set; } = null; // null = auto (OS language)

  // Create a copy to change the hotkey or language without changing the current settings.
  // Any lists are shared between the copy and the original settings.
  public AppSettings Copy() => (AppSettings)MemberwiseClone();
}
