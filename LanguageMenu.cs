// SPDX-License-Identifier: MIT

internal static class LanguageMenu
{
  public static ToolStripMenuItem Build(Action<string?> changeLanguage)
  {
    var menu = new ToolStripMenuItem(LocalizationManager.Get("Language"));
    menu.DropDownItems.Add(new ToolStripMenuItem(LocalizationManager.Get("SystemDefault"), null,
      (_, _) => changeLanguage(null))
    {
      Checked = SettingsManager.CurrentAppSettings.Language == null
    });
    menu.DropDownItems.Add(new ToolStripSeparator());

    foreach (var code in LocalizationManager.GetSupportedLanguages()
      .OrderBy(LocalizationManager.GetLanguageDisplayName))
    {
      menu.DropDownItems.Add(new ToolStripMenuItem(LocalizationManager.GetLanguageDisplayName(code), null,
        (_, _) => changeLanguage(code))
      {
        Checked = SettingsManager.CurrentAppSettings.Language == code
      });
    }

    return menu;
  }
}
