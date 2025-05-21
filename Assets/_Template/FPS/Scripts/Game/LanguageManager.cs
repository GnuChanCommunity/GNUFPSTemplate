using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // Required for file operations
using System.Linq; // Required for LINQ operations like ToList, Split

namespace Unity.FPS.Game
{
    /// <summary>
    /// Get language dictionaries from json files. Example game.tr.json or game.en.json
    /// and provide lang object with singleton pattern according to selected language.
    /// Get selected language from gamelang.env
    /// Get available languages from gamelang.env
    /// </summary>
    public static class LanguageManager
    {
        // Helper classes for JSON deserialization with JsonUtility
        [System.Serializable]
        private class TranslationEntry
        {
            public string key;
            public string value;
        }

        [System.Serializable]
        private class LanguageDataContainer
        {
            public List<TranslationEntry> entries = new List<TranslationEntry>();
        }

        /// <summary>
        /// Holds the translations for a specific language.
        /// </summary>
        public class Lang
        {
            private Dictionary<string, string> _translations;

            public Lang(Dictionary<string, string> translations)
            {
                _translations = translations ?? new Dictionary<string, string>();
            }

            /// <summary>
            /// Gets a translated string for the given key.
            /// </summary>
            /// <param name="key">The key for the translation.</param>
            /// <param name="defaultValue">Value to return if key is not found. If null, returns the key itself.</param>
            /// <returns>The translated string, or defaultValue, or the key if not found and no defaultValue.</returns>
            public string Get(string key, string defaultValue = null)
            {
                if (_translations.TryGetValue(key, out string value))
                {
                    return value;
                }
                Debug.LogWarning($"LanguageManager: Key '{key}' not found for language '{SelectedLanguageCode}'. " +
                                 (defaultValue != null ? $"Returning default value '{defaultValue}'." : "Returning key."));
                return defaultValue ?? key; // Return key itself if no default is provided
            }

            /// <summary>
            /// Checks if a translation key exists.
            /// </summary>
            public bool Has(string key)
            {
                return _translations.ContainsKey(key);
            }
        }

        private static Lang _currentLang;
        public static Lang Current
        {
            get
            {
                if (_currentLang == null)
                {
                    Debug.LogWarning("LanguageManager.Current accessed before initialization or after a failed load. Attempting to initialize.");
                    Initialize(); // Try to initialize if not already
                    if (_currentLang == null)
                    {
                        Debug.LogError("LanguageManager: Failed to initialize. Returning an empty Lang object to prevent null reference errors.");
                        _currentLang = new Lang(new Dictionary<string, string>()); // Fallback to empty
                    }
                }
                return _currentLang;
            }
            private set => _currentLang = value;
        }

        public static string SelectedLanguageCode { get; private set; } = "en"; // Default to 'en'
        public static List<string> AvailableLanguageCodes { get; private set; } = new List<string>();

        private const string EnvFileName = "gamelang.env";
        private const string LangFilePrefix = "game.";
        private const string LangFileSuffix = ".json";
        private const string SelectedLanguageKeyInEnv = "SELECTED_LANGUAGE";
        private const string AvailableLanguagesKeyInEnv = "AVAILABLE_LANGUAGES";

        // Automatically initialize when the game loads
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            LoadEnvConfig();
            LoadLanguage(SelectedLanguageCode);
        }

        private static void LoadEnvConfig()
        {
            string envFilePath = Path.Combine(Application.streamingAssetsPath, EnvFileName);
            if (File.Exists(envFilePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(envFilePath);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) // Skip empty lines and comments
                            continue;

                        string[] parts = line.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            if (key == SelectedLanguageKeyInEnv)
                            {
                                SelectedLanguageCode = value;
                                Debug.Log($"LanguageManager: Selected language from env: {SelectedLanguageCode}");
                            }
                            else if (key == AvailableLanguagesKeyInEnv)
                            {
                                AvailableLanguageCodes = value.Split(',').Select(s => s.Trim()).ToList();
                                Debug.Log($"LanguageManager: Available languages from env: {string.Join(", ", AvailableLanguageCodes)}");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"LanguageManager: Error reading or parsing '{EnvFileName}': {ex.Message}. Using defaults.");
                }
            }
            else
            {
                Debug.LogWarning($"LanguageManager: '{EnvFileName}' not found in StreamingAssets. Using default selected language '{SelectedLanguageCode}'. Available languages might be empty.");
                // Optionally, populate AvailableLanguageCodes with a default if the file is missing
                if (!AvailableLanguageCodes.Contains(SelectedLanguageCode))
                {
                    AvailableLanguageCodes.Add(SelectedLanguageCode);
                }
            }
            // Ensure selected language is in available, if available are defined
            if (AvailableLanguageCodes.Any() && !AvailableLanguageCodes.Contains(SelectedLanguageCode))
            {
                Debug.LogWarning($"LanguageManager: Selected language '{SelectedLanguageCode}' not in available languages. Defaulting to first available or 'en'.");
                SelectedLanguageCode = AvailableLanguageCodes.FirstOrDefault() ?? "en";
            }
        }

        private static bool LoadLanguage(string langCode)
        {
            string langFilePath = Path.Combine(Application.streamingAssetsPath, $"{LangFilePrefix}{langCode}{LangFileSuffix}");
            Debug.Log($"LanguageManager: Attempting to load language file: {langFilePath}");

            if (File.Exists(langFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(langFilePath);
                    LanguageDataContainer dataContainer = JsonUtility.FromJson<LanguageDataContainer>(jsonString);

                    if (dataContainer == null || dataContainer.entries == null)
                    {
                        Debug.LogError($"LanguageManager: Failed to parse JSON or 'entries' array is missing/null in '{langFilePath}'.");
                        Current = new Lang(new Dictionary<string, string>()); // Empty lang
                        return false;
                    }

                    Dictionary<string, string> translations = new Dictionary<string, string>();
                    foreach (var entry in dataContainer.entries)
                    {
                        if (!string.IsNullOrEmpty(entry.key))
                        {
                            translations[entry.key] = entry.value;
                        }
                        else
                        {
                            Debug.LogWarning($"LanguageManager: Found an entry with a null or empty key in '{langFilePath}'. Skipping.");
                        }
                    }
                    Current = new Lang(translations);
                    SelectedLanguageCode = langCode; // Update selected language code upon successful load
                    Debug.Log($"LanguageManager: Successfully loaded language '{langCode}' with {translations.Count} entries.");
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"LanguageManager: Error loading or parsing language file '{langFilePath}': {ex.Message}");
                    Current = new Lang(new Dictionary<string, string>()); // Fallback to empty
                    return false;
                }
            }
            else
            {
                // If not exist then initialize files with default settings
                try
                {
                    // Ensure StreamingAssets directory exists
                    Directory.CreateDirectory(Application.streamingAssetsPath);

                    // Create default env file if missing
                    string envFilePath = Path.Combine(Application.streamingAssetsPath, EnvFileName);
                    if (!File.Exists(envFilePath))
                    {
                        string defaultEnv = $"{SelectedLanguageKeyInEnv}=en\n{AvailableLanguagesKeyInEnv}=en";
                        File.WriteAllText(envFilePath, defaultEnv);
                        Debug.Log($"LanguageManager: Created default '{EnvFileName}' at {envFilePath}");
                    }

                    // Create default language file if missing
                    string defaultLangFilePath = Path.Combine(Application.streamingAssetsPath, $"{LangFilePrefix}en{LangFileSuffix}");
                    if (!File.Exists(defaultLangFilePath))
                    {
                        // Minimal default JSON structure
                        string defaultJson = "{\"entries\":[{\"key\":\"hello\",\"value\":\"Hello\"},{\"key\":\"welcome\",\"value\":\"Welcome\"}]}";
                        File.WriteAllText(defaultLangFilePath, defaultJson);
                        Debug.Log($"LanguageManager: Created default language file at {defaultLangFilePath}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"LanguageManager: Error creating default language/env files: {ex.Message}");
                }

                Debug.LogError($"LanguageManager: Language file not found: {langFilePath}");
                if (_currentLang == null) // Only set to empty if no language was previously loaded
                {
                    Current = new Lang(new Dictionary<string, string>());
                }
                return false;
            }
        }

        /// <summary>
        /// Switches the current language. Reloads translations from the corresponding JSON file.
        /// </summary>
        /// <param name="langCode">The language code (e.g., "en", "tr").</param>
        /// <returns>True if language switched successfully, false otherwise.</returns>
        public static bool SwitchLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode))
            {
                Debug.LogWarning("LanguageManager: SwitchLanguage called with null or empty langCode.");
                return false;
            }

            if (AvailableLanguageCodes.Any() && !AvailableLanguageCodes.Contains(langCode))
            {
                Debug.LogWarning($"LanguageManager: Language '{langCode}' is not in the list of available languages. Attempting to load anyway.");
                // Optionally, you might want to prevent loading if not in AvailableLanguageCodes
            }

            if (SelectedLanguageCode == langCode && _currentLang != null)
            {
                Debug.Log($"LanguageManager: Language '{langCode}' is already selected.");
                return true; // Already selected
            }

            bool success = LoadLanguage(langCode);
            if (success)
            {
                // Optionally, save the new SelectedLanguageCode to gamelang.env if you want persistence
                // SaveSelectedLanguageToEnv(langCode); // Implement this if needed
                OnLanguageChanged?.Invoke();
            }
            else
            {
                Debug.LogError($"LanguageManager: Failed to switch to language '{langCode}'. Previous language '{SelectedLanguageCode}' might still be active if it was loaded.");
            }
            return success;
        }

        /// <summary>
        /// Static method to get a translated string directly.
        /// </summary>
        /// <param name="key">The key for the translation.</param>
        /// <param name="defaultValue">Optional default value if key is not found.</param>
        /// <returns>The translated string.</returns>
        public static string GetText(string key, string defaultValue = null)
        {
            return Current.Get(key, defaultValue);
        }

        /// <summary>
        /// Event triggered when the language is successfully changed.
        /// UI elements can subscribe to this to update their text.
        /// </summary>
        public static event System.Action OnLanguageChanged;


        // --- Example of how to save selected language back to .env (Optional) ---
        /*
        private static void SaveSelectedLanguageToEnv(string langCode)
        {
            string envFilePath = Path.Combine(Application.streamingAssetsPath, EnvFileName);
            List<string> lines = new List<string>();
            bool selectedKeyFound = false;

            if (File.Exists(envFilePath))
            {
                lines.AddRange(File.ReadAllLines(envFilePath));
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].TrimStart().StartsWith(SelectedLanguageKeyInEnv + "="))
                    {
                        lines[i] = $"{SelectedLanguageKeyInEnv}={langCode}";
                        selectedKeyFound = true;
                        break;
                    }
                }
            }

            if (!selectedKeyFound)
            {
                lines.Add($"{SelectedLanguageKeyInEnv}={langCode}");
            }

            try
            {
                File.WriteAllLines(envFilePath, lines);
                Debug.Log($"LanguageManager: Saved selected language '{langCode}' to '{EnvFileName}'.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"LanguageManager: Error saving selected language to '{EnvFileName}': {ex.Message}");
            }
        }
        */
    }
}