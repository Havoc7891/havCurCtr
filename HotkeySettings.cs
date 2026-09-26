// SPDX-License-Identifier: MIT

internal static class HotkeySettings
{
  public const uint Alt = 0x0001;
  public const uint Control = 0x0002;
  public const uint Shift = 0x0004;
  public const uint Win = 0x0008;
  public const uint DefaultModifiers = Control | Alt;
  public const int DefaultKey = (int)Keys.C;

  public static bool IsValid(uint modifiers, int key)
  {
    if ((modifiers & ~(Alt | Control | Shift | Win)) != 0 || key < 0x08 || key > 0xFE)
    {
      return false;
    }

    var keyboardKey = (Keys)key;
    return Enum.IsDefined(keyboardKey) && keyboardKey is not
      (Keys.ShiftKey or Keys.ControlKey or Keys.Menu or
       Keys.LShiftKey or Keys.RShiftKey or Keys.LControlKey or Keys.RControlKey or
       Keys.LMenu or Keys.RMenu or Keys.LWin or Keys.RWin or Keys.F12);
  }

  public static string Format(uint modifiers, int key)
  {
    var parts = new List<string>();
    if ((modifiers & Control) != 0)
    {
      parts.Add(LocalizationManager.Get("ModifierCtrl"));
    }
    if ((modifiers & Alt) != 0)
    {
      parts.Add(LocalizationManager.Get("ModifierAlt"));
    }
    if ((modifiers & Shift) != 0)
    {
      parts.Add(LocalizationManager.Get("ModifierShift"));
    }
    if ((modifiers & Win) != 0)
    {
      parts.Add(LocalizationManager.Get("ModifierWin"));
    }
    parts.Add(FormatKey((Keys)key));

    return string.Join(" + ", parts);
  }

  public static string FormatKey(Keys key)
  {
    if (key is >= Keys.D0 and <= Keys.D9)
    {
      return ((int)key - (int)Keys.D0).ToString();
    }

    return new KeysConverter().ConvertToString(key) ?? key.ToString();
  }
}
