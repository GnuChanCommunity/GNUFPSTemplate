using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for LINQ operations like Select
using TMPro;
using Unity.FPS.Game; // Assuming GameSettingsManager is in this namespace
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Manages the UI elements for game settings, interacting with the GameSettingsManager.
    /// It populates UI elements with current settings and updates the GameSettingsManager
    /// when UI elements are changed by the user.
    /// </summary>
    public class GameSettingUIManager : MonoBehaviour
    {
        [Tooltip("Reference to the GameSettingsManager instance.")]
        [SerializeField] private GameSettingsManager gameSettingsManager;

        [Header("Graphic Settings UI Elements")]
        [Tooltip("Dropdown for selecting screen resolution and refresh rate.")]
        [SerializeField] private TMP_Dropdown resolutionDropDown;

        [Tooltip("Dropdown for selecting texture quality.")]
        [SerializeField] private TMP_Dropdown textureQualityDropDown; // Corrected typo

        [Tooltip("Dropdown for selecting anti-aliasing level.")]
        [SerializeField] private TMP_Dropdown antiAliasingDropDown; // Corrected variable name

        [Tooltip("Toggle for enabling or disabling VSync.")]
        [SerializeField] private Toggle vsyncToggle;

        [Tooltip("Toggle for enabling or disabling fullscreen mode.")] // Added for fullscreen
        [SerializeField] private Toggle fullscreenToggle;

        // (Optional UI Elements - not in your original draft, but your GameSettingsManager supports them)
        // [Tooltip("Dropdown for selecting quality preset.")]
        // [SerializeField] private TMP_Dropdown qualityPresetDropDown;
        //
        // [Tooltip("Dropdown for selecting window mode.")]
        // [SerializeField] private TMP_Dropdown windowModeDropDown;

        [Header("Sound Settings UI Elements")]
        [Tooltip("Slider for master volume control.")]
        [SerializeField] private Slider masterVolumeSlider;

        [Tooltip("Slider for music volume control.")]
        [SerializeField] private Slider musicVolumeSlider;

        [Tooltip("Slider for SFX volume control.")]
        [SerializeField] private Slider sfxVolumeSlider;


        [Header("Language Preferences UI Elements")]
        [Tooltip("Dropdown for subtitle or title language selection")]
        [SerializeField] private TMP_Dropdown langTitleSelectionDropdown; // This is a standard UI.Dropdown

        [Tooltip("Voice language selection dropdown")]
        [SerializeField] private TMP_Dropdown audioLangSelectionDropdown; // I will configure later, configuration for sounds will be in different code.

        [Tooltip("Subtitle enable disable toggle")]
        [SerializeField] private Toggle subtitleToggle; // I will configure it later :')

        // Private fields for managing UI state and mappings
        private Resolution[] availableResolutions;
        private List<string> resolutionOptionsList = new List<string>();

        // Anti-aliasing mapping: UI text to actual AA value
        private readonly Dictionary<int, string> antiAliasingOptions = new Dictionary<int, string>
        {
            {0, "Off"},
            {2, "2x MSAA"},
            {4, "4x MSAA"},
            {8, "8x MSAA"}
        };
        private List<int> antiAliasingValues = new List<int>(); // To map dropdown index to AA value

        // Texture quality mapping: UI text to actual quality level (0-3)
        private readonly string[] textureQualityTexts = { "Full (Native)", "Half", "Quarter", "Eighth" };


        // --- UNITY LIFECYCLE METHODS ---

        void Awake()
        {
            if (gameSettingsManager == null)
            {
                Debug.LogError("GameSettingUIManager: GameSettingsManager is not assigned!");
                enabled = false; // Disable this component if manager is missing
                return;
            }
        }
        void OnEnable()
        {
            StartCoroutine(WaitForGameSettingsAndInitialize());
        }

        private IEnumerator WaitForGameSettingsAndInitialize()
        {
            // gameSettingsManager null değil mi kontrolü
            if (gameSettingsManager == null)
            {
                Debug.LogError("GameSettingsManager atanmamış!");
                yield break;
            }

            // Ayarlar yüklenene kadar bekle
            while (!gameSettingsManager.IsInitialized)
            {
                yield return null; // bir frame bekle
            }

            PopulateUIElements();
            RegisterUIListeners();
            RefreshAllUIValues();
        }

        void OnDisable()
        {
            if (gameSettingsManager == null) return;

            UnregisterUIListeners();
        }

        // --- UI POPULATION AND INITIALIZATION ---

        /// <summary>
        /// Populates dropdowns with available options.
        /// This should be called once, or if options can change dynamically.
        /// </summary>
        private void PopulateUIElements()
        {
            // Populate Resolution Dropdown
            if (resolutionDropDown != null)
            {
                if (gameSettingsManager == null)
                {
                    Debug.LogError("gameSettingsManager is null!");
                    return;
                }

                availableResolutions = gameSettingsManager.GetAvailableResolutions();
                if (availableResolutions == null)
                {
                    Debug.LogError("availableResolutions is null!");
                    return;
                }

                resolutionOptionsList.Clear();
                resolutionDropDown.ClearOptions();

                var uniqueResolutions = availableResolutions
                    .Select(res => new { res.width, res.height, refresh = (int)res.refreshRateRatio.value })
                    .Distinct()
                    .OrderByDescending(r => r.width)
                    .ThenByDescending(r => r.height)
                    .ThenByDescending(r => r.refresh);

                foreach (var res in uniqueResolutions)
                {
                    string optionText = $"{res.width} x {res.height} @ {res.refresh} Hz";
                    resolutionOptionsList.Add(optionText);
                }
                resolutionDropDown.AddOptions(resolutionOptionsList);
            }


            // Populate Texture Quality Dropdown
            if (textureQualityDropDown != null)
            {
                textureQualityDropDown.ClearOptions();
                textureQualityDropDown.AddOptions(textureQualityTexts.ToList());
            }

            // Populate Anti-Aliasing Dropdown
            if (antiAliasingDropDown != null)
            {
                antiAliasingDropDown.ClearOptions();
                antiAliasingValues = antiAliasingOptions.Keys.ToList(); // Store the AA values (0,2,4,8)
                antiAliasingDropDown.AddOptions(antiAliasingOptions.Values.ToList()); // Store the AA text ("Off", "2x"...)
            }

            // --- ADDED FOR LANGUAGE UI ---
            // Populate Title/Subtitle Language Dropdown
            if (langTitleSelectionDropdown != null)
            {
                langTitleSelectionDropdown.ClearOptions();
                List<string> availableLangCodes = gameSettingsManager.GetAvailableLanguageCodes();
                if (availableLangCodes != null && availableLangCodes.Any())
                {
                    // Optional: Convert codes to more user-friendly names if you have a mapping
                    // For now, using codes directly.
                    langTitleSelectionDropdown.AddOptions(availableLangCodes);
                }
                else
                {
                    Debug.LogWarning("GameSettingUIManager: No available languages found to populate dropdown.");
                    langTitleSelectionDropdown.AddOptions(new List<string> { "N/A" }); // Placeholder
                }
            }

            // Populate Audio Language Dropdown (Placeholder for now)
            if (audioLangSelectionDropdown != null)
            {
                audioLangSelectionDropdown.ClearOptions();
                // TODO: Populate with actual audio language options when implemented
                audioLangSelectionDropdown.AddOptions(new List<string> { "Default (To be configured)" });
            }
            // --- END OF ADDED FOR LANGUAGE UI ---

            // (Optional: Populate Quality Preset Dropdown)
            // if (qualityPresetDropDown != null)
            // {
            //     qualityPresetDropDown.ClearOptions();
            //     qualityPresetDropDown.AddOptions(gameSettingsManager.GetQualityPresetNames().ToList());
            // }

            // (Optional: Populate Window Mode Dropdown)
            // if (windowModeDropDown != null)
            // {
            //     windowModeDropDown.ClearOptions();
            //     windowModeDropDown.AddOptions(System.Enum.GetNames(typeof(FullScreenMode)).ToList());
            // }
        }

        /// <summary>
        /// Registers listeners for UI element value changes.
        /// </summary>
        private void RegisterUIListeners()
        {
            if (resolutionDropDown != null) resolutionDropDown.onValueChanged.AddListener(OnResolutionChanged);
            if (textureQualityDropDown != null) textureQualityDropDown.onValueChanged.AddListener(OnTextureQualityChanged);
            if (antiAliasingDropDown != null) antiAliasingDropDown.onValueChanged.AddListener(OnAntiAliasingChanged);
            if (vsyncToggle != null) vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // --- ADDED FOR LANGUAGE UI ---
            if (langTitleSelectionDropdown != null) langTitleSelectionDropdown.onValueChanged.AddListener(OnTitleLanguageChanged);
            if (audioLangSelectionDropdown != null) audioLangSelectionDropdown.onValueChanged.AddListener(OnAudioLanguageChanged); // Placeholder
            if (subtitleToggle != null) subtitleToggle.onValueChanged.AddListener(OnSubtitleToggleChanged); // Placeholder
            // --- END OF ADDED FOR LANGUAGE UI ---

            // (Optional Listeners)
            // if (qualityPresetDropDown != null) qualityPresetDropDown.onValueChanged.AddListener(OnQualityPresetChanged);
            // if (windowModeDropDown != null) windowModeDropDown.onValueChanged.AddListener(OnWindowModeChanged);
        }


        /// <summary>
        /// Unregisters listeners to prevent errors when the GameObject is disabled or destroyed.
        /// </summary>
        private void UnregisterUIListeners()
        {
            if (resolutionDropDown != null) resolutionDropDown.onValueChanged.RemoveListener(OnResolutionChanged);
            if (textureQualityDropDown != null) textureQualityDropDown.onValueChanged.RemoveListener(OnTextureQualityChanged);
            if (antiAliasingDropDown != null) antiAliasingDropDown.onValueChanged.RemoveListener(OnAntiAliasingChanged);
            if (vsyncToggle != null) vsyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullScreenToggleChanged);

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

            // --- ADDED FOR LANGUAGE UI ---
            if (langTitleSelectionDropdown != null) langTitleSelectionDropdown.onValueChanged.RemoveListener(OnTitleLanguageChanged);
            if (audioLangSelectionDropdown != null) audioLangSelectionDropdown.onValueChanged.RemoveListener(OnAudioLanguageChanged);
            if (subtitleToggle != null) subtitleToggle.onValueChanged.RemoveListener(OnSubtitleToggleChanged);
            // --- END OF ADDED FOR LANGUAGE UI ---

            // (Optional Listeners)
            // if (qualityPresetDropDown != null) qualityPresetDropDown.onValueChanged.RemoveListener(OnQualityPresetChanged);
            // if (windowModeDropDown != null) windowModeDropDown.onValueChanged.RemoveListener(OnWindowModeChanged);
        }

        // --- UI VALUE REFRESH ---

        /// <summary>
        /// Updates all UI elements to reflect the current settings from GameSettingsManager.
        /// Temporarily removes listeners to prevent Set... methods from triggering OnValueChanged events.
        /// </summary>
        public void RefreshAllUIValues()
        {
            if (gameSettingsManager == null) return;

            UnregisterUIListeners(); // Avoid triggering change events while setting values

            // Refresh Graphics Settings
            if (resolutionDropDown != null && availableResolutions != null)
            {
                int currentWidth = gameSettingsManager.GetResolutionWidth();
                int currentHeight = gameSettingsManager.GetResolutionHeight();
                int currentRefreshRate = gameSettingsManager.GetRefreshRate();
                string currentResString = $"{currentWidth} x {currentHeight} @ {currentRefreshRate} Hz";

                int resolutionIndex = resolutionOptionsList.FindIndex(r => r == currentResString);
                if (resolutionIndex != -1)
                {
                    resolutionDropDown.value = resolutionIndex;
                }
                else
                {
                    Debug.LogWarning($"GameSettingUIManager: Current resolution {currentResString} not found in dropdown. Defaulting selection.");
                    if (resolutionDropDown.options.Count > 0) resolutionDropDown.value = 0;
                }
                resolutionDropDown.RefreshShownValue();
            }

            if (textureQualityDropDown != null)
            {
                textureQualityDropDown.value = gameSettingsManager.GetTextureQuality();
                textureQualityDropDown.RefreshShownValue();
            }

            if (antiAliasingDropDown != null)
            {
                int currentAALevel = gameSettingsManager.GetAntiAliasing();
                int aaIndex = antiAliasingValues.IndexOf(currentAALevel);
                if (aaIndex != -1)
                {
                    antiAliasingDropDown.value = aaIndex;
                }
                else
                {
                    Debug.LogWarning($"GameSettingUIManager: Current AA level {currentAALevel} not mapped in dropdown. Defaulting selection.");
                    if (antiAliasingDropDown.options.Count > 0) antiAliasingDropDown.value = 0; // Default to "Off"
                }
                antiAliasingDropDown.RefreshShownValue();
            }

            if (vsyncToggle != null)
            {
                vsyncToggle.isOn = gameSettingsManager.GetVSync();
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = gameSettingsManager.GetWindowMode() == FullScreenMode.FullScreenWindow || gameSettingsManager.GetWindowMode() == FullScreenMode.ExclusiveFullScreen;
            }

            // (Optional: Refresh Quality Preset)
                // if (qualityPresetDropDown != null)
                // {
                //     qualityPresetDropDown.value = gameSettingsManager.GetQualityPresetIndex();
                //     qualityPresetDropDown.RefreshShownValue();
                // }

                // (Optional: Refresh Window Mode)
                // if (windowModeDropDown != null)
                // {
                //     windowModeDropDown.value = (int)gameSettingsManager.GetWindowMode();
                //     windowModeDropDown.RefreshShownValue();
                // }

            // Refresh Audio Settings
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = gameSettingsManager.GetMasterVolume();
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = gameSettingsManager.GetMusicVolume();
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = gameSettingsManager.GetSFXVolume();
            }

            // --- ADDED FOR LANGUAGE UI ---
            // Refresh Title/Subtitle Language Dropdown
            if (langTitleSelectionDropdown != null)
            {
                string currentLangCode = gameSettingsManager.GetSelectedLanguageCode();
                if (!string.IsNullOrEmpty(currentLangCode))
                {
                    // Find the index of the current language code in the dropdown options
                    int langIndex = -1;
                    for (int i = 0; i < langTitleSelectionDropdown.options.Count; i++)
                    {
                        if (langTitleSelectionDropdown.options[i].text == currentLangCode)
                        {
                            langIndex = i;
                            break;
                        }
                    }

                    if (langIndex != -1)
                    {
                        langTitleSelectionDropdown.value = langIndex;
                    }
                    else
                    {
                        Debug.LogWarning($"GameSettingUIManager: Current language code '{currentLangCode}' not found in dropdown. Defaulting selection.");
                        if (langTitleSelectionDropdown.options.Count > 0) langTitleSelectionDropdown.value = 0;
                    }
                }
                else if (langTitleSelectionDropdown.options.Count > 0)
                {
                     langTitleSelectionDropdown.value = 0; // Default to first if no language set
                }
                langTitleSelectionDropdown.RefreshShownValue();
            }

            // Refresh Audio Language Dropdown (Placeholder)
            if (audioLangSelectionDropdown != null)
            {
                // TODO: Set to current audio language when implemented
                if(audioLangSelectionDropdown.options.Count > 0) audioLangSelectionDropdown.value = 0;
                audioLangSelectionDropdown.RefreshShownValue();
            }

            // Refresh Subtitle Toggle (Placeholder)
            if (subtitleToggle != null)
            {
                // TODO: Set based on current subtitle setting when implemented
                subtitleToggle.isOn = true; // Default to on for now
            }
            // --- END OF ADDED FOR LANGUAGE UI ---

            RegisterUIListeners(); // Re-register listeners
        }


        // --- UI EVENT HANDLERS (Called by UI elements) ---

        // Graphics Settings Handlers
        public void OnResolutionChanged(int index)
        {
            if (gameSettingsManager == null || availableResolutions == null || index < 0 || index >= resolutionOptionsList.Count) return;

            string selectedOption = resolutionOptionsList[index];
            string[] parts = selectedOption.Split(new[] { " x ", " @ ", " Hz" }, System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int width) &&
                int.TryParse(parts[1], out int height) &&
                int.TryParse(parts[2], out int refreshRate))
            {
                gameSettingsManager.SetResolution(width, height, refreshRate);
            }
            else
            {
                Debug.LogError($"GameSettingUIManager: Could not parse resolution string: {selectedOption}");
            }
        }

        public void OnTextureQualityChanged(int index)
        {
            if (gameSettingsManager == null) return;
            gameSettingsManager.SetTextureQuality(index);
        }

        public void OnAntiAliasingChanged(int index)
        {
            if (gameSettingsManager == null || index < 0 || index >= antiAliasingValues.Count) return;
            gameSettingsManager.SetAntiAliasing(antiAliasingValues[index]);
        }

        public void OnVSyncChanged(bool isOn)
        {
            if (gameSettingsManager == null) return;
            gameSettingsManager.SetVSync(isOn);
        }


        private void OnFullScreenToggleChanged(bool isOn)
        {
            if (gameSettingsManager == null) return;
            // Assuming FullScreenWindow is your preferred "fullscreen" and Windowed for "not fullscreen"
            // You might want to offer ExclusiveFullScreen as well if desired.
            gameSettingsManager.SetWindowMode(isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        // (Optional Handlers)
        // public void OnQualityPresetChanged(int index)
        // {
        //     if (gameSettingsManager == null) return;
        //     gameSettingsManager.SetQualityPreset(index);
        //     RefreshAllUIValues();
        // }
        //
        // public void OnWindowModeChanged(int index)
        // {
        //     if (gameSettingsManager == null) return;
        //     gameSettingsManager.SetWindowMode((FullScreenMode)index);
        // }

        // Audio Settings Handlers
        public void OnMasterVolumeChanged(float value)
        {
            if (gameSettingsManager == null) return;
            gameSettingsManager.SetMasterVolume(value);
        }

        public void OnMusicVolumeChanged(float value)
        {
            if (gameSettingsManager == null) return;
            gameSettingsManager.SetMusicVolume(value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            if (gameSettingsManager == null) return;
            gameSettingsManager.SetSFXVolume(value);
        }

        // --- ADDED FOR LANGUAGE UI ---
        // Language Settings Handlers
        public void OnTitleLanguageChanged(int index)
        {
            if (gameSettingsManager == null || langTitleSelectionDropdown == null || index < 0 || index >= langTitleSelectionDropdown.options.Count)
            {
                return;
            }

            string selectedLangCode = langTitleSelectionDropdown.options[index].text;
            if (selectedLangCode == "N/A") return; // Ignore placeholder

            Debug.Log($"GameSettingUIManager: Title language changed to index {index}, code: {selectedLangCode}");
            gameSettingsManager.SetLanguage(selectedLangCode);
        }

        public void OnAudioLanguageChanged(int index)
        {
            if (gameSettingsManager == null) return;
            // TODO: Implement when audio language selection is fully integrated into GameSettingsManager
            Debug.Log($"GameSettingUIManager: Audio language changed to index {index}. (Placeholder - Not fully implemented yet)");
            // string selectedAudioLang = audioLangSelectionDropdown.options[index].text;
            // gameSettingsManager.SetAudioLanguage(selectedAudioLang); // Example future call
        }

        public void OnSubtitleToggleChanged(bool isOn)
        {
            if (gameSettingsManager == null) return;
            // TODO: Implement when subtitle toggle is fully integrated into GameSettingsManager
            Debug.Log($"GameSettingUIManager: Subtitle toggle changed to {isOn}. (Placeholder - Not fully implemented yet)");
            // gameSettingsManager.SetSubtitlesEnabled(isOn); // Example future call
        }
        // --- END OF ADDED FOR LANGUAGE UI ---


        // --- BUTTON HANDLERS ---

        public void SaveApplyButton()
        {
            if (gameSettingsManager == null) return;
            Debug.Log("GameSettingUIManager: Settings are continuously saved. 'Apply' button pressed (no explicit action needed here).");
        }

        public void ResetToDefaultButton()
        {
            if (gameSettingsManager == null) return;
            gameSettingsManager.ResetToDefaults();
            RefreshAllUIValues();
        }

        public void ExitButton()
        {
            this.gameObject.SetActive(false);
        }
    }
}