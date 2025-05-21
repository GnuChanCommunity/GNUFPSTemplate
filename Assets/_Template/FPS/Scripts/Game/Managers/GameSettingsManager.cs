using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // Required for file operations
using UnityEngine.Audio; // Required for AudioMixer
// --- ADDED FOR LANGUAGE INTEGRATION ---
// Assuming LanguageManager is in the global namespace or accessible
// If LanguageManager is in a namespace, you'll need:
// using YourNamespace.LanguageManager; // or whatever its namespace is

/// <summary>
/// This manager controls master volume, graphic settings, language, etc.
/// It allows saving settings to a JSON file and loading them back.
/// Settings are applied at startup and can be changed at runtime.
/// </summary>
namespace Unity.FPS.Game
{
    public class GameSettingsManager : MonoBehaviour
    {
        // ------------- INNER CLASS FOR SETTINGS DATA ------------- //

        /// <summary>
        /// Holds all game settings that can be saved and loaded.
        /// </summary>
        [System.Serializable]
        public class GameSettings
        {
            // Graphics Settings
            public bool vsyncEnabled;
            public int antiAliasingLevel; // 0, 2, 4, 8 typically
            public int textureQuality; // 0: Full, 1: Half, 2: Quarter, 3: Eighth
            public int resolutionWidth;
            public int resolutionHeight;
            public int refreshRate;
            public FullScreenMode windowMode;
            public int qualityPresetIndex; // Index for Unity's Quality Settings presets

            // Audio Settings
            public float masterVolume; // Range 0.0 to 1.0
            public float musicVolume;  // Range 0.0 to 1.0
            public float sfxVolume;    // Range 0.0 to 1.0

            // --- ADDED FOR LANGUAGE INTEGRATION ---
            // Language Setting
            public string selectedLanguageCode; // e.g., "en", "tr"
            // --- END OF ADDED FOR LANGUAGE INTEGRATION ---


            public void LoadDefaultSettings()
            {
                // Graphics Defaults
                vsyncEnabled = true;
                antiAliasingLevel = 2;
                textureQuality = 0; // Full resolution
                resolutionWidth = Screen.currentResolution.width;
                resolutionHeight = Screen.currentResolution.height;
                refreshRate = (int)Screen.currentResolution.refreshRateRatio.value;
                windowMode = FullScreenMode.FullScreenWindow; // Native fullscreen usually best
                qualityPresetIndex = QualitySettings.GetQualityLevel(); // Start with current project quality

                // Audio Defaults
                masterVolume = 0.8f;
                musicVolume = 0.7f;
                sfxVolume = 0.75f;

                // --- ADDED FOR LANGUAGE INTEGRATION ---
                // Language Default
                // LanguageManager should be initialized by this point due to RuntimeInitializeOnLoadMethod.
                // Use its initially selected language (from .env or its own internal default).
                selectedLanguageCode = LanguageManager.SelectedLanguageCode;
                if (string.IsNullOrEmpty(selectedLanguageCode))
                {
                    // Fallback if LanguageManager.SelectedLanguageCode was somehow not set
                    selectedLanguageCode = "en"; // Or your preferred absolute default
                    Debug.LogWarning("GameSettings: LanguageManager.SelectedLanguageCode was empty during default settings load, defaulting language to 'en'.");
                }
                // --- END OF ADDED FOR LANGUAGE INTEGRATION ---
            }
        }

        // ------------- PUBLIC PROPERTIES & FIELDS ------------- //

        public GameSettings currentSettings { get; private set; }

        // Assign your game's main AudioMixer in the Inspector
        [Header("Audio Configuration")]
        [Tooltip("The main AudioMixer for controlling game audio levels.")]
        public AudioMixer mainAudioMixer;

        // Exposed names for AudioMixer parameters (must match names in your AudioMixer)
        public const string MASTER_VOLUME_PARAM = "MasterVolume";
        public const string MUSIC_VOLUME_PARAM = "MusicVolume";
        public const string SFX_VOLUME_PARAM = "SFXVolume";


         public bool IsInitialized { get; private set; } = false;

        // ------------- PRIVATE FIELDS ------------- //

        private string settingsFilePath;
        private const string SETTINGS_FILE_NAME = "gamesettings.json";

        // Available resolutions list
        private Resolution[] availableResolutions;

        // ------------- UNITY LIFECYCLE METHODS ------------- //

        void Awake()
        {
            // Define the path for the settings file
            settingsFilePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);

            // Populate available resolutions
            availableResolutions = Screen.resolutions;

            // Load settings from file, or use defaults if no file exists
            LoadSettings();
        }

        void Start()
        {
            // Apply loaded settings once everything is initialized
            // This is in Start() to ensure all other systems (like AudioMixer, LanguageManager) might be ready
            ApplyAllSettings();
            IsInitialized = true;
        }

        // ------------- SETTINGS MANAGEMENT: LOAD, SAVE, APPLY ------------- //

        /// <summary>
        /// Loads settings from the JSON file. If the file doesn't exist, it creates default settings.
        /// </summary>
        public void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(settingsFilePath);
                    currentSettings = JsonUtility.FromJson<GameSettings>(json);
                    // --- ADDED FOR LANGUAGE INTEGRATION ---
                    // Ensure loaded language code is not null/empty, fallback if it is.
                    if (currentSettings != null && string.IsNullOrEmpty(currentSettings.selectedLanguageCode))
                    {
                        Debug.LogWarning("GameSettingsManager: Loaded settings have an empty language code. Falling back to LanguageManager's current or default.");
                        currentSettings.selectedLanguageCode = LanguageManager.SelectedLanguageCode;
                        if (string.IsNullOrEmpty(currentSettings.selectedLanguageCode))
                        {
                            currentSettings.selectedLanguageCode = "en"; // Absolute fallback
                        }
                    }
                    // --- END OF ADDED FOR LANGUAGE INTEGRATION ---
                    Debug.Log("GameSettingsManager: Settings loaded successfully from " + settingsFilePath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("GameSettingsManager: Error loading settings file. Using defaults. Error: " + e.Message);
                    InitializeDefaultSettings();
                }
            }
            else
            {
                Debug.Log("GameSettingsManager: No settings file found. Initializing with default settings.");
                InitializeDefaultSettings();
                // Optionally save defaults immediately
                // SaveSettings();
            }
        }

        /// <summary>
        /// Saves the current settings to the JSON file.
        /// </summary>
        public void SaveSettings()
        {
            if (currentSettings == null)
            {
                Debug.LogError("GameSettingsManager: currentSettings is null. Cannot save.");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(currentSettings, true); // 'true' for pretty print
                File.WriteAllText(settingsFilePath, json);
                Debug.Log("GameSettingsManager: Settings saved successfully to " + settingsFilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("GameSettingsManager: Error saving settings file. Error: " + e.Message);
            }
        }

        /// <summary>
        /// Initializes currentSettings with default values.
        /// </summary>
        private void InitializeDefaultSettings()
        {
            currentSettings = new GameSettings();
            currentSettings.LoadDefaultSettings();
        }

        /// <summary>
        /// Applies all current settings to the game.
        /// </summary>
        public void ApplyAllSettings()
        {
            if (currentSettings == null)
            {
                Debug.LogError("GameSettingsManager: currentSettings is null. Cannot apply settings.");
                return;
            }

            // Apply Graphics Settings
            ApplyVSync(currentSettings.vsyncEnabled);
            ApplyAntiAliasing(currentSettings.antiAliasingLevel);
            ApplyTextureQuality(currentSettings.textureQuality);
            ApplyQualityPreset(currentSettings.qualityPresetIndex); // Apply preset first as it might override some individual settings
            ApplyResolutionAndWindowMode(currentSettings.resolutionWidth, currentSettings.resolutionHeight, currentSettings.refreshRate, currentSettings.windowMode);

            // Apply Audio Settings
            ApplyMasterVolume(currentSettings.masterVolume);
            ApplyMusicVolume(currentSettings.musicVolume);
            ApplySFXVolume(currentSettings.sfxVolume);

            // --- ADDED FOR LANGUAGE INTEGRATION ---
            // Apply Language Setting
            ApplyLanguage(currentSettings.selectedLanguageCode);
            // --- END OF ADDED FOR LANGUAGE INTEGRATION ---

            Debug.Log("GameSettingsManager: All settings have been applied.");
        }


        // ------------- INDIVIDUAL SETTING APPLICATION METHODS ------------- //

        /// <summary>
        /// Applies VSync setting.
        /// </summary>
        /// <param name="enabled">True to enable VSync, false to disable.</param>
        private void ApplyVSync(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            Debug.Log($"GameSettingsManager: VSync set to {enabled} (vSyncCount: {QualitySettings.vSyncCount})");
        }

        /// <summary>
        /// Applies Anti-Aliasing level.
        /// </summary>
        /// <param name="level">Anti-Aliasing level (0, 2, 4, 8).</param>
        private void ApplyAntiAliasing(int level)
        {
            // Ensure the level is valid (0, 2, 4, 8 are common AA sample levels)
            if (level == 0 || level == 2 || level == 4 || level == 8)
            {
                QualitySettings.antiAliasing = level;
                Debug.Log($"GameSettingsManager: Anti-Aliasing set to {level}x");
            }
            else
            {
                Debug.LogWarning($"GameSettingsManager: Invalid Anti-Aliasing level: {level}. Using current setting ({QualitySettings.antiAliasing}x).");
            }
        }

        /// <summary>
        /// Applies Texture Quality.
        /// </summary>
        /// <param name="quality">Texture quality level (0: Full, 1: Half, 2: Quarter, 3: Eighth).</param>
        private void ApplyTextureQuality(int quality)
        {
            // masterTextureLimit: 0 = full res, 1 = half, 2 = quarter, 3 = eighth
            if (quality >= 0 && quality <= 3)
            {
                QualitySettings.globalTextureMipmapLimit = quality; // Renamed from masterTextureLimit in newer Unity versions.
                                                              // For older Unity, use: QualitySettings.masterTextureLimit = quality;
                Debug.Log($"GameSettingsManager: Texture Quality set to level {quality} (Mipmap Limit: {QualitySettings.globalTextureMipmapLimit})");
            }
            else
            {
                Debug.LogWarning($"GameSettingsManager: Invalid Texture Quality level: {quality}. Using current setting ({QualitySettings.globalTextureMipmapLimit}).");
            }
        }

        /// <summary>
        /// Applies Resolution and Window Mode.
        /// </summary>
        private void ApplyResolutionAndWindowMode(int width, int height, int refresh, FullScreenMode mode)
        {
            // Validate resolution against available resolutions if necessary, though Screen.SetResolution handles invalid ones gracefully.
            Screen.SetResolution(width, height, mode, new RefreshRate { numerator = (uint)refresh, denominator = 1}); // For newer Unity versions
            // For older Unity versions: Screen.SetResolution(width, height, mode, refresh);
            Debug.Log($"GameSettingsManager: Resolution set to {width}x{height} @ {refresh}Hz, Window Mode: {mode}");
        }

        /// <summary>
        /// Applies a Unity Quality Preset.
        /// </summary>
        /// <param name="presetIndex">Index of the quality preset.</param>
        private void ApplyQualityPreset(int presetIndex)
        {
            if (presetIndex >= 0 && presetIndex < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(presetIndex, true); // true to apply expensive changes
                Debug.Log($"GameSettingsManager: Quality Preset set to '{QualitySettings.names[presetIndex]}' (Index: {presetIndex})");
            }
            else
            {
                Debug.LogWarning($"GameSettingsManager: Invalid Quality Preset Index: {presetIndex}. Using current ({QualitySettings.GetQualityLevel()}).");
            }
        }

        /// <summary>
        /// Applies Master Volume. Converts linear volume (0-1) to logarithmic (dB) for AudioMixer.
        /// </summary>
        /// <param name="volume">Volume level (0.0 to 1.0).</param>
        private void ApplyMasterVolume(float volume)
        {
            if (mainAudioMixer != null)
            {
                float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f; // -80dB is effectively silence
                mainAudioMixer.SetFloat(MASTER_VOLUME_PARAM, dB);
                Debug.Log($"GameSettingsManager: Master Volume set to {volume:P0} ({dB:F2} dB)");
            }
            else
            {
                AudioListener.volume = Mathf.Clamp01(volume);
                Debug.LogWarning("GameSettingsManager: MainAudioMixer not assigned. Setting global AudioListener.volume for Master Volume.");
            }
        }

        /// <summary>
        /// Applies Music Volume.
        /// </summary>
        private void ApplyMusicVolume(float volume)
        {
            if (mainAudioMixer != null)
            {
                float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
                mainAudioMixer.SetFloat(MUSIC_VOLUME_PARAM, dB);
                Debug.Log($"GameSettingsManager: Music Volume set to {volume:P0} ({dB:F2} dB)");
            }
            else
            {
                Debug.LogWarning("GameSettingsManager: MainAudioMixer not assigned. Cannot set Music Volume.");
            }
        }

        /// <summary>
        /// Applies SFX Volume.
        /// </summary>
        private void ApplySFXVolume(float volume)
        {
            if (mainAudioMixer != null)
            {
                float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
                mainAudioMixer.SetFloat(SFX_VOLUME_PARAM, dB);
                Debug.Log($"GameSettingsManager: SFX Volume set to {volume:P0} ({dB:F2} dB)");
            }
            else
            {
                Debug.LogWarning("GameSettingsManager: MainAudioMixer not assigned. Cannot set SFX Volume.");
            }
        }

        // --- ADDED FOR LANGUAGE INTEGRATION ---
        /// <summary>
        /// Applies the language setting using LanguageManager.
        /// </summary>
        /// <param name="langCode">The language code (e.g., "en", "tr").</param>
        private void ApplyLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode))
            {
                Debug.LogWarning("GameSettingsManager: Attempted to apply an empty or null language code. No change made.");
                return;
            }

            // LanguageManager should be initialized and Current should be available.
            // If LanguageManager.Current is null here, it indicates an issue with LanguageManager's initialization.
            if (LanguageManager.Current == null)
            {
                Debug.LogError("GameSettingsManager: LanguageManager is not properly initialized. Cannot apply language setting.");
                // Attempt re-initialization if necessary, or ensure it's robustly initialized before GameSettingsManager.Start()
                // LanguageManager.Initialize(); // Be cautious with re-initializing here.
                return;
            }

            // Only switch if the new language code is different from the LanguageManager's current selection.
            if (LanguageManager.SelectedLanguageCode != langCode)
            {
                bool switched = LanguageManager.SwitchLanguage(langCode);
                if (switched)
                {
                    Debug.Log($"GameSettingsManager: Language successfully set to '{langCode}' via LanguageManager.");
                }
                else
                {
                    // If switching failed, the LanguageManager might have fallen back or kept the old one.
                    // Update currentSettings.selectedLanguageCode to reflect what LanguageManager actually has.
                    currentSettings.selectedLanguageCode = LanguageManager.SelectedLanguageCode;
                    Debug.LogWarning($"GameSettingsManager: Failed to switch language to '{langCode}'. Language remains '{currentSettings.selectedLanguageCode}'.");
                }
            }
            else
            {
                Debug.Log($"GameSettingsManager: Language '{langCode}' is already active in LanguageManager. No change needed.");
            }
        }
        // --- END OF ADDED FOR LANGUAGE INTEGRATION ---


        // ------------- PUBLIC SETTER METHODS FOR UI/GAMEPLAY INTERACTION ------------- //
        // These methods update the currentSettings object and then call Save and Apply.

        public void SetVSync(bool enabled)
        {
            if (currentSettings.vsyncEnabled == enabled) return;
            currentSettings.vsyncEnabled = enabled;
            ApplyVSync(enabled);
            SaveSettings();
        }

        public void SetAntiAliasing(int level)
        {
            if (currentSettings.antiAliasingLevel == level) return;
            currentSettings.antiAliasingLevel = level;
            ApplyAntiAliasing(level);
            SaveSettings();
        }

        public void SetTextureQuality(int quality)
        {
            if (currentSettings.textureQuality == quality) return;
            currentSettings.textureQuality = quality;
            ApplyTextureQuality(quality);
            SaveSettings();
        }

        public void SetResolution(int width, int height, int refreshRate)
        {
            if (currentSettings.resolutionWidth == width &&
                currentSettings.resolutionHeight == height &&
                currentSettings.refreshRate == refreshRate) return;

            currentSettings.resolutionWidth = width;
            currentSettings.resolutionHeight = height;
            currentSettings.refreshRate = refreshRate; 
            ApplyResolutionAndWindowMode(currentSettings.resolutionWidth, currentSettings.resolutionHeight, currentSettings.refreshRate, currentSettings.windowMode);
            SaveSettings();
        }

        public void SetWindowMode(FullScreenMode mode)
        {
            if (currentSettings.windowMode == mode) return;
            currentSettings.windowMode = mode;
            ApplyResolutionAndWindowMode(currentSettings.resolutionWidth, currentSettings.resolutionHeight, currentSettings.refreshRate, currentSettings.windowMode);
            SaveSettings();
        }

        public void SetQualityPreset(int presetIndex)
        {
            if (currentSettings.qualityPresetIndex == presetIndex) return;
            currentSettings.qualityPresetIndex = presetIndex;
            ApplyQualityPreset(presetIndex);
            SaveSettings();
        }

        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp01(volume); 
            if (Mathf.Approximately(currentSettings.masterVolume, volume)) return;
            currentSettings.masterVolume = volume;
            ApplyMasterVolume(volume);
            SaveSettings();
        }

        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (Mathf.Approximately(currentSettings.musicVolume, volume)) return;
            currentSettings.musicVolume = volume;
            ApplyMusicVolume(volume);
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (Mathf.Approximately(currentSettings.sfxVolume, volume)) return;
            currentSettings.sfxVolume = volume;
            ApplySFXVolume(volume);
            SaveSettings();
        }

        // --- ADDED FOR LANGUAGE INTEGRATION ---
        /// <summary>
        /// Sets the game's language. Updates current settings, applies the change, and saves.
        /// </summary>
        /// <param name="langCode">The language code to switch to (e.g., "en", "tr").</param>
        public void SetLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode))
            {
                Debug.LogWarning("GameSettingsManager: SetLanguage called with an empty or null language code.");
                return;
            }
            if (currentSettings.selectedLanguageCode == langCode && LanguageManager.SelectedLanguageCode == langCode)
            {
                Debug.Log($"GameSettingsManager: Language '{langCode}' is already selected and active.");
                return; // Already set and applied
            }

            string previousLangCode = currentSettings.selectedLanguageCode;
            currentSettings.selectedLanguageCode = langCode;
            ApplyLanguage(langCode); // This will call LanguageManager.SwitchLanguage

            // Check if LanguageManager successfully switched or reverted
            if (LanguageManager.SelectedLanguageCode == langCode)
            {
                SaveSettings(); // Save only if successfully switched or confirmed
            }
            else
            {
                // LanguageManager might have failed and reverted or selected a default.
                // Update our settings to reflect what LanguageManager actually has.
                currentSettings.selectedLanguageCode = LanguageManager.SelectedLanguageCode;
                Debug.LogWarning($"GameSettingsManager: LanguageManager did not switch to '{langCode}'. Language is now '{currentSettings.selectedLanguageCode}'. Settings not saved with requested code.");
                // Optionally, you could still save currentSettings which now reflects the actual language.
                // SaveSettings();
            }
        }
        // --- END OF ADDED FOR LANGUAGE INTEGRATION ---


        // ------------- PUBLIC GETTER METHODS FOR UI ------------- //

        public Resolution[] GetAvailableResolutions()
        {
            return availableResolutions;
        }

        public string[] GetQualityPresetNames()
        {
            return QualitySettings.names;
        }

        public bool GetVSync() => currentSettings.vsyncEnabled;
        public int GetAntiAliasing() => currentSettings.antiAliasingLevel;
        public int GetTextureQuality() => currentSettings.textureQuality;
        public int GetResolutionWidth() => currentSettings.resolutionWidth;
        public int GetResolutionHeight() => currentSettings.resolutionHeight;
        public int GetRefreshRate() => currentSettings.refreshRate;
        public FullScreenMode GetWindowMode() => currentSettings.windowMode;
        public int GetQualityPresetIndex() => currentSettings.qualityPresetIndex;
        public float GetMasterVolume() => currentSettings.masterVolume;
        public float GetMusicVolume() => currentSettings.musicVolume;
        public float GetSFXVolume() => currentSettings.sfxVolume;

        // --- ADDED FOR LANGUAGE INTEGRATION ---
        /// <summary>
        /// Gets the currently selected language code from settings.
        /// </summary>
        /// <returns>The language code string (e.g., "en", "tr").</returns>
        public string GetSelectedLanguageCode() => currentSettings?.selectedLanguageCode;

        /// <summary>
        /// Gets the list of available language codes directly from LanguageManager.
        /// Useful for populating UI dropdowns.
        /// </summary>
        public List<string> GetAvailableLanguageCodes() => LanguageManager.AvailableLanguageCodes;
        // --- END OF ADDED FOR LANGUAGE INTEGRATION ---


        // ------------- UTILITY ------------- //

        /// <summary>
        /// Resets all settings to their default values, applies them, and saves.
        /// </summary>
        public void ResetToDefaults()
        {
            Debug.Log("GameSettingsManager: Resetting settings to defaults.");
            InitializeDefaultSettings(); 
            ApplyAllSettings();
            SaveSettings();
        }
    }
}