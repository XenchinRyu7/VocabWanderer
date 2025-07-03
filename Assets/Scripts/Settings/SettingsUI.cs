using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Music Volume")]
    public Slider musicSlider;
    public InputField musicPercentInput; // InputField (Inner GameObject)

    [Header("SFX Volume")]
    public Slider sfxSlider;
    public InputField sfxPercentInput; // InputField (Inner GameObject)

    [Header("Ambience Volume")]
    public Slider ambienceSlider;
    public InputField ambiencePercentInput; // InputField (Inner GameObject)

    [Header("Controls")]
    public Button saveButton;

    private bool isLoadingSettings = false;

    void Start()
    {
        // Debug: Check if UI elements are assigned
        Debug.Log("=== UI Elements Check ===");
        Debug.Log($"musicSlider: {(musicSlider != null ? "OK" : "NULL")}");
        Debug.Log($"musicPercentInput: {(musicPercentInput != null ? "OK" : "NULL")}");
        Debug.Log($"sfxSlider: {(sfxSlider != null ? "OK" : "NULL")}");
        Debug.Log($"sfxPercentInput: {(sfxPercentInput != null ? "OK" : "NULL")}");
        Debug.Log($"ambienceSlider: {(ambienceSlider != null ? "OK" : "NULL")}");
        Debug.Log($"ambiencePercentInput: {(ambiencePercentInput != null ? "OK" : "NULL")}");

        InitializeUI();
        SetupSaveButton();
        SetupSliderEvents();
        SetupInputFieldEvents();
    }

    void SetupSliderEvents()
    {
        // Setup slider events via script (lebih reliable)
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        if (ambienceSlider != null)
            ambienceSlider.onValueChanged.AddListener(OnAmbienceSliderChanged);
        Debug.Log("Slider events setup complete via script");
    }

    void SetupInputFieldEvents()
    {
        // Setup InputField events
        if (musicPercentInput != null)
            musicPercentInput.onEndEdit.AddListener(OnMusicInputChanged);
        if (sfxPercentInput != null)
            sfxPercentInput.onEndEdit.AddListener(OnSFXInputChanged);
        if (ambiencePercentInput != null)
            ambiencePercentInput.onEndEdit.AddListener(OnAmbienceInputChanged);
        Debug.Log("InputField events setup complete via script");
    }

    void InitializeUI()
    {
        // Setup slider ranges untuk memastikan 0-100
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 100f;
        }
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 100f;
        }
        if (ambienceSlider != null)
        {
            ambienceSlider.minValue = 0f;
            ambienceSlider.maxValue = 100f;
        }
        // Tunggu AudioSettingsManager ready
        if (AudioSettingsManager.Instance != null)
        {
            var settings = AudioSettingsManager.Instance.currentSettings;

            // DISABLE events temporarily saat load settings
            isLoadingSettings = true;

            // Set slider values (0-1 -> 0-100)
            musicSlider.value = settings.musicVolume * 100f;
            sfxSlider.value = settings.sfxVolume * 100f;
            ambienceSlider.value = settings.ambienceVolume * 100f;

            // Update InputField values (yang otomatis update text component di dalamnya)
            UpdateMusicInput(settings.musicVolume * 100f);
            UpdateSFXInput(settings.sfxVolume * 100f);
            UpdateAmbienceInput(settings.ambienceVolume * 100f);

            // RE-ENABLE events
            isLoadingSettings = false;

            Debug.Log(
                $"Settings UI initialized with loaded values - Music: {settings.musicVolume}, SFX: {settings.sfxVolume}, Ambience: {settings.ambienceVolume}"
            );
        }
        else
        {
            Debug.LogWarning("AudioSettingsManager not found, using default values");

            // Set default values
            musicSlider.value = 100f;
            sfxSlider.value = 100f;
            ambienceSlider.value = 100f;

            // Update InputField dengan default values
            UpdateMusicInput(100f);
            UpdateSFXInput(100f);
            UpdateAmbienceInput(100f);
        }
    }

    void SetupSaveButton()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
            Debug.Log("Save button setup complete");
        }
    } // Music Slider Event - Update text + apply temporary (tidak save)

    public void OnMusicSliderChanged(float sliderValue)
    {
        // SKIP jika sedang loading settings untuk prevent circular update
        if (isLoadingSettings)
            return;

        // Update InputField (yang otomatis update text di dalamnya)
        UpdateMusicInput(sliderValue);

        // Apply volume temporary (tanpa save ke JSON)
        float volumeValue = sliderValue / 100f;
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance.SetMusicVolumeTemporary(volumeValue); // Quick check if immediate apply worked
            if (BacksoundPlayer.instance != null)
            {
                Debug.Log($"[SLIDER] Music volume applied: {volumeValue}");
            }
        }

        Debug.Log($"Music slider: {sliderValue}% (temporary)");
    }

    public void OnSFXSliderChanged(float sliderValue)
    {
        if (isLoadingSettings)
            return;

        UpdateSFXInput(sliderValue);

        float volumeValue = sliderValue / 100f;
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance.SetSFXVolumeTemporary(volumeValue); // Quick check if immediate apply worked
            if (BacksoundPlayer.instance != null)
            {
                Debug.Log($"[SLIDER] SFX volume applied: {volumeValue}");
            }
        }

        Debug.Log($"SFX slider moved to: {sliderValue}% (temporary)");
    }

    public void OnAmbienceSliderChanged(float sliderValue)
    {
        if (isLoadingSettings)
            return;

        // Update InputField (yang otomatis update text di dalamnya)
        UpdateAmbienceInput(sliderValue);

        // Apply volume temporary (tanpa save ke JSON)
        float volumeValue = sliderValue / 100f;
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance.SetAmbienceVolumeTemporary(volumeValue); // Quick check if immediate apply worked
            if (BacksoundPlayer.instance != null)
            {
                Debug.Log($"[SLIDER] Ambience volume applied: {volumeValue}");
            }
        }

        Debug.Log($"Ambience slider moved to: {sliderValue}% (temporary)");
    }

    // Update InputField methods (otomatis update text component di dalamnya)
    void UpdateMusicInput(float percentage)
    {
        if (musicPercentInput != null)
        {
            string newValue = Mathf.RoundToInt(percentage).ToString();
            musicPercentInput.text = newValue;
            Debug.Log($"[DEBUG] Music InputField updated to: '{newValue}'");
        }
        else
        {
            Debug.LogError("[DEBUG] musicPercentInput is NULL!");
        }
    }

    void UpdateSFXInput(float percentage)
    {
        if (sfxPercentInput != null)
        {
            string newValue = Mathf.RoundToInt(percentage).ToString();
            sfxPercentInput.text = newValue;
            Debug.Log($"[DEBUG] SFX InputField updated to: '{newValue}'");
        }
        else
        {
            Debug.LogError("[DEBUG] sfxPercentInput is NULL!");
        }
    }

    void UpdateAmbienceInput(float percentage)
    {
        if (ambiencePercentInput != null)
        {
            string newValue = Mathf.RoundToInt(percentage).ToString();
            ambiencePercentInput.text = newValue;
            Debug.Log($"[DEBUG] Ambience InputField updated to: '{newValue}'");
        }
        else
        {
            Debug.LogError("[DEBUG] ambiencePercentInput is NULL!");
        }
    }

    // InputField event handlers
    public void OnMusicInputChanged(string input)
    {
        if (float.TryParse(input, out float value))
        {
            value = Mathf.Clamp(value, 0f, 100f);
            Debug.Log($"[INPUT] Music input changed to: {value}");
            musicSlider.value = value;
            // OnMusicSliderChanged akan dipanggil otomatis
        }
        else
        {
            Debug.LogWarning($"[INPUT] Invalid music input: '{input}'");
        }
    }

    public void OnSFXInputChanged(string input)
    {
        if (float.TryParse(input, out float value))
        {
            value = Mathf.Clamp(value, 0f, 100f);
            Debug.Log($"[INPUT] SFX input changed to: {value}");
            sfxSlider.value = value;
            // OnSFXSliderChanged akan dipanggil otomatis
        }
        else
        {
            Debug.LogWarning($"[INPUT] Invalid SFX input: '{input}'");
        }
    }

    public void OnAmbienceInputChanged(string input)
    {
        if (float.TryParse(input, out float value))
        {
            value = Mathf.Clamp(value, 0f, 100f);
            Debug.Log($"[INPUT] Ambience input changed to: {value}");
            ambienceSlider.value = value;
            // OnAmbienceSliderChanged akan dipanggil otomatis
        }
        else
        {
            Debug.LogWarning($"[INPUT] Invalid ambience input: '{input}'");
        }
    }

    // Method untuk reset ke default (optional)
    public void ResetToDefault()
    {
        musicSlider.value = 100f;
        sfxSlider.value = 100f;
        ambienceSlider.value = 100f;
        Debug.Log("Audio settings reset to default");
    } // Save Button Event - Simpan current settings ke JSON (PERMANENT) + FORCE APPLY

    public void OnSaveButtonClicked()
    {
        Debug.Log("=== SAVE BUTTON CLICKED ===");

        // Quick debug: Find active AudioSources
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        int playingCount = 0;
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (allAudioSources[i].isPlaying)
                playingCount++;
        }
        Debug.Log(
            $"[DEBUG] Found {allAudioSources.Length} AudioSources in scene ({playingCount} playing)"
        );

        if (AudioSettingsManager.Instance != null)
        {
            // Current settings sudah ter-update dari slider temporary
            // Tinggal save ke JSON saja
            AudioSettingsManager.Instance.SaveSettings();

            // FORCE APPLY ke BacksoundPlayer setelah save
            Debug.Log("Forcing audio settings apply after save...");
            AudioSettingsManager.Instance.ApplySettingsToAudio();

            // EXTRA: Force immediate volume update tanpa fade
            StartCoroutine(ForceImmediateVolumeUpdate());

            Debug.Log(
                "Audio settings saved to JSON: "
                    + $"Music: {AudioSettingsManager.Instance.currentSettings.musicVolume}, "
                    + $"SFX: {AudioSettingsManager.Instance.currentSettings.sfxVolume}, "
                    + $"Ambience: {AudioSettingsManager.Instance.currentSettings.ambienceVolume}"
            );
        }
        else
        {
            Debug.LogError("AudioSettingsManager not found!");
        }
    }

    // Force immediate volume update tanpa menunggu fade
    private System.Collections.IEnumerator ForceImmediateVolumeUpdate()
    {
        yield return new WaitForSeconds(0.1f); // Wait sedikit untuk apply selesai

        Debug.Log("[FORCE] Starting brute force volume update...");

        // BRUTE FORCE: Update ALL AudioSources di scene
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        Debug.Log($"[FORCE] Found {allAudioSources.Length} AudioSources, updating all...");

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            var audioSrc = allAudioSources[i];
            if (audioSrc != null && AudioSettingsManager.Instance != null)
            {
                var settings = AudioSettingsManager.Instance.currentSettings;

                // Update music volume untuk semua AudioSource yang playing music
                if (audioSrc.clip != null && audioSrc.isPlaying)
                {
                    float oldVolume = audioSrc.volume;
                    audioSrc.volume = settings.musicVolume;
                    Debug.Log(
                        $"[FORCE] Updated AudioSource[{i}] '{audioSrc.gameObject.name}' volume: {oldVolume} → {audioSrc.volume}"
                    );
                }
            }
        }

        // Original method untuk BacksoundPlayer.instance
        if (BacksoundPlayer.instance != null && AudioSettingsManager.Instance != null)
        {
            var settings = AudioSettingsManager.Instance.currentSettings;

            // FORCE update volume langsung ke AudioSource
            if (BacksoundPlayer.instance.GetComponent<AudioSource>() != null)
            {
                var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
                audioSource.volume = settings.musicVolume;
                Debug.Log(
                    $"[FORCE] BacksoundPlayer AudioSource volume directly set to: {audioSource.volume}"
                );
            }

            // Double-check dengan log
            Debug.Log(
                $"[FORCE] Final verification - BacksoundPlayer musicVolume field: {BacksoundPlayer.instance.musicVolume}"
            );
        }

        Debug.Log("[FORCE] Brute force volume update complete!");
    }
}
