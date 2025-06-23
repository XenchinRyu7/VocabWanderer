using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Music Volume")]
    public Slider musicSlider;
    public Text musicPercentText;

    [Header("SFX Volume")]
    public Slider sfxSlider;
    public Text sfxPercentText;

    [Header("Ambience Volume")]
    public Slider ambienceSlider;
    public Text ambiencePercentText;

    [Header("Controls")]
    public Button saveButton; // Ganti nama dari applyButton ke saveButton

    // Flag untuk mencegah circular update saat load settings
    private bool isLoadingSettings = false;

    void Start()
    {
        InitializeUI();
        SetupSaveButton();
        SetupSliderEvents(); // RESTORE - karena Inspector setup tidak bekerja
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

            // Update text percentage
            UpdateMusicText(settings.musicVolume * 100f);
            UpdateSFXText(settings.sfxVolume * 100f);
            UpdateAmbienceText(settings.ambienceVolume * 100f);

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

            UpdateMusicText(100f);
            UpdateSFXText(100f);
            UpdateAmbienceText(100f);
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

        // Update text realtime
        UpdateMusicText(sliderValue); // Apply volume temporary (tanpa save ke JSON)
        float volumeValue = sliderValue / 100f;
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance.SetMusicVolumeTemporary(volumeValue);            // Quick check if immediate apply worked
            if (BacksoundPlayer.instance != null)
            {
                Debug.Log($"[SLIDER] Music volume applied: {volumeValue}");
            }
        }

        Debug.Log($"Music slider: {sliderValue}% (temporary)");
    }

    // SFX Slider Event - Update text + apply temporary (tidak save)
    public void OnSFXSliderChanged(float sliderValue)
    {
        // SKIP jika sedang loading settings untuk prevent circular update
        if (isLoadingSettings)
            return;

        // Update text realtime
        UpdateSFXText(sliderValue); // Apply volume temporary (tanpa save ke JSON)
        float volumeValue = sliderValue / 100f;
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance.SetSFXVolumeTemporary(volumeValue);            // Quick check if immediate apply worked
            if (BacksoundPlayer.instance != null)
            {
                Debug.Log($"[SLIDER] SFX volume applied: {volumeValue}");
            }
        }

        Debug.Log($"SFX slider moved to: {sliderValue}% (temporary)");
    }

    // Ambience Slider Event - Update text + apply temporary (tidak save)
    public void OnAmbienceSliderChanged(float sliderValue)
    {
        // SKIP jika sedang loading settings untuk prevent circular update
        if (isLoadingSettings)
            return;

        // Update text realtime
        UpdateAmbienceText(sliderValue); // Apply volume temporary (tanpa save ke JSON)
        float volumeValue = sliderValue / 100f;
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance.SetAmbienceVolumeTemporary(volumeValue);            // Quick check if immediate apply worked
            if (BacksoundPlayer.instance != null)
            {
                Debug.Log($"[SLIDER] Ambience volume applied: {volumeValue}");
            }
        }

        Debug.Log($"Ambience slider moved to: {sliderValue}% (temporary)");
    }

    // Update text methods
    void UpdateMusicText(float percentage)
    {
        if (musicPercentText != null)
            musicPercentText.text = Mathf.RoundToInt(percentage) + "%";
    }

    void UpdateSFXText(float percentage)
    {
        if (sfxPercentText != null)
            sfxPercentText.text = Mathf.RoundToInt(percentage) + "%";
    }

    void UpdateAmbienceText(float percentage)
    {
        if (ambiencePercentText != null)
            ambiencePercentText.text = Mathf.RoundToInt(percentage) + "%";
    }

    // Method untuk reset ke default (optional)
    public void ResetToDefault()
    {
        musicSlider.value = 100f;
        sfxSlider.value = 100f;
        ambienceSlider.value = 100f;
        Debug.Log("Audio settings reset to default");
    }    // Save Button Event - Simpan current settings ke JSON (PERMANENT) + FORCE APPLY
    public void OnSaveButtonClicked()
    {
        Debug.Log("=== SAVE BUTTON CLICKED ===");

        // Quick debug: Find active AudioSources
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        int playingCount = 0;
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (allAudioSources[i].isPlaying) playingCount++;
        }
        Debug.Log($"[DEBUG] Found {allAudioSources.Length} AudioSources in scene ({playingCount} playing)");

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
                    Debug.Log($"[FORCE] Updated AudioSource[{i}] '{audioSrc.gameObject.name}' volume: {oldVolume} → {audioSrc.volume}");
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
                Debug.Log($"[FORCE] BacksoundPlayer AudioSource volume directly set to: {audioSource.volume}");
            }

            // Double-check dengan log
            Debug.Log($"[FORCE] Final verification - BacksoundPlayer musicVolume field: {BacksoundPlayer.instance.musicVolume}");
        }

        Debug.Log("[FORCE] Brute force volume update complete!");
    }
}
