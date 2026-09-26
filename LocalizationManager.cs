// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text.Json;

internal static class LocalizationManager
{
  private static readonly Dictionary<string, string> _fallback = [];
  private static readonly Dictionary<string, string> _active = [];

  public static string CurrentLanguage { get; private set; } = "en";

  public static void Initialize(string? preferredLanguage)
  {
    _fallback.Clear();

    foreach (var entry in LoadEmbeddedEnglish())
    {
      _fallback[entry.Key] = entry.Value;
    }

    if (LoadFromFile("en", out var english))
    {
      foreach (var entry in english)
      {
        _fallback[entry.Key] = entry.Value;
      }
    }

    _active.Clear();

    CurrentLanguage = "en";

    string languageCode = preferredLanguage ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    if (!string.Equals(languageCode, "en", StringComparison.OrdinalIgnoreCase)
        && LoadFromFile(languageCode, out var translation))
    {
      foreach (var entry in translation)
      {
        _active[entry.Key] = entry.Value;
      }

      CurrentLanguage = languageCode;
    }
  }

  public static string Get(string key, params object?[] args)
  {
    string value = _active.TryGetValue(key, out var translated) ? translated
        : _fallback.TryGetValue(key, out var fallback) ? fallback : key;

    if (args.Length == 0)
    {
      return value;
    }

    try
    {
      return string.Format(CultureInfo.CurrentCulture, value, args);
    }
    catch (FormatException)
    {
      return value;
    }
  }

  public static IReadOnlyList<string> GetSupportedLanguages()
  {
    var languages = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "en" };

    try
    {
      if (Directory.Exists(LocalizationPaths.Folder))
      {
        foreach (string path in Directory.GetFiles(LocalizationPaths.Folder, "*.json"))
        {
          string languageCode = Path.GetFileNameWithoutExtension(path);

          if (IsLanguageCode(languageCode))
          {
            languages.Add(languageCode);
          }
        }
      }
    }
    catch (IOException)
    {
      // English remains available if the translation folder can't be read
    }
    catch (UnauthorizedAccessException)
    {
      // English remains available if the translation folder can't be read
    }

    return [.. languages.OrderBy(language => language, StringComparer.OrdinalIgnoreCase)];
  }

  public static string GetLanguageDisplayName(string languageCode)
  {
    try
    {
      return CultureInfo.GetCultureInfo(languageCode).NativeName;
    }
    catch (CultureNotFoundException)
    {
      return languageCode;
    }
  }

  private static bool IsLanguageCode(string languageCode)
  {
    return !string.IsNullOrWhiteSpace(languageCode)
        && languageCode.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
  }

  private static Dictionary<string, string> LoadEmbeddedEnglish()
  {
    using var stream = typeof(LocalizationManager).Assembly.GetManifestResourceStream("languages.en.json");
    return stream is not null && LoadFromStream(stream, out var translation) ? translation : [];
  }

  private static bool LoadFromFile(string languageCode, out Dictionary<string, string> translation)
  {
    translation = [];

    if (!IsLanguageCode(languageCode))
    {
      return false;
    }

    try
    {
      string path = Path.Combine(LocalizationPaths.Folder, $"{languageCode}.json");
      using var stream = File.OpenRead(path);
      return LoadFromStream(stream, out translation);
    }
    catch (IOException)
    {
      return false;
    }
    catch (UnauthorizedAccessException)
    {
      return false;
    }
  }

  private static bool LoadFromStream(Stream stream, out Dictionary<string, string> translation)
  {
    translation = [];

    try
    {
      var values = JsonSerializer.Deserialize<Dictionary<string, string?>>(stream);

      if (values is null)
      {
        return false;
      }

      foreach (var entry in values)
      {
        if (entry.Value is not null)
        {
          translation[entry.Key] = entry.Value;
        }
      }

      return true;
    }
    catch (IOException)
    {
      return false;
    }
    catch (JsonException)
    {
      return false;
    }
  }
}
