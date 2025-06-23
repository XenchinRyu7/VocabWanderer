using System.Collections;
using System.IO;
using UnityEngine;

[System.Serializable]
public class AudioSettingsData
{
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public float ambienceVolume = 1.0f;
}

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance;

    [Header("Current Settings")]
    public AudioSettingsData currentSettings;

    private string saveFilePath;

    void Awake()
    {
        // Singleton pattern dengan handling scene transition yang lebih aman
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[AudioSettingsManager] Created new instance");
            InitializeSettings();
        }
        else if (Instance != this)
        {
            Debug.Log("[AudioSettingsManager] Duplicate instance found, destroying new one");
            Destroy(gameObject);
            return;
        }

        // Jika instance sudah ada dan ini adalah instance yang sama,
        // pastikan settings tetap ter-apply di scene baru
        if (Instance == this)
        {
            Debug.Log("[AudioSettingsManager] Same instance in new scene, re-applying settings");
            StartCoroutine(ReapplySettingsForNewScene());
        }
    }

    void InitializeSettings()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "AudioSettings.json");
        Debug.Log("Audio settings path: " + saveFilePath);

        LoadSettings();

        StartCoroutine(ApplySettingsWithDelay());
    }

    private System.Collections.IEnumerator ApplySettingsWithDelay()
    {
        yield return null;

        int attempts = 0;
        while (BacksoundPlayer.instance == null && attempts < 10)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (BacksoundPlayer.instance != null)
        {
            ApplySettingsToAudio();
            Debug.Log("Audio settings applied after delay");
        }
        else
        {
            Debug.LogWarning("BacksoundPlayer not found after waiting");
        }
    }

    // Method khusus untuk re-apply settings saat scene transition
    private System.Collections.IEnumerator ReapplySettingsForNewScene()
    {
        Debug.Log("[SCENE-TRANSITION] Waiting for new scene to be ready...");

        // Tunggu beberapa frame untuk memastikan scene sudah fully loaded
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[SCENE-TRANSITION] Re-applying audio settings to new scene...");
        ApplySettingsToAudio();

        // Extra: Cek apakah BacksoundPlayer masih hidup dan playing
        if (BacksoundPlayer.instance != null)
        {
            var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                Debug.Log(
                    $"[SCENE-TRANSITION] BacksoundPlayer status - isPlaying: {audioSource.isPlaying}, volume: {audioSource.volume}"
                );

                // Jika tidak playing, coba start ulang musik
                if (!audioSource.isPlaying && audioSource.clip != null)
                {
                    Debug.Log(
                        "[SCENE-TRANSITION] BacksoundPlayer not playing, attempting to restart..."
                    );
                    audioSource.Play();
                }
            }
        }
        else
        {
            Debug.LogWarning(
                "[SCENE-TRANSITION] BacksoundPlayer instance is NULL after scene transition!"
            );
        }
    }

    public void LoadSettings()
    {
        Debug.Log("=== LOADING AUDIO SETTINGS ===");
        Debug.Log("File path: " + saveFilePath);
        Debug.Log("File exists: " + File.Exists(saveFilePath));

        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                Debug.Log("JSON content: " + jsonData);

                currentSettings = JsonUtility.FromJson<AudioSettingsData>(jsonData);

                Debug.Log(
                    $"Loaded settings - Music: {currentSettings.musicVolume}, SFX: {currentSettings.sfxVolume}, Ambience: {currentSettings.ambienceVolume}"
                );
                Debug.Log("Audio settings loaded successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load audio settings: " + e.Message);
                CreateDefaultSettings();
            }
        }
        else
        {
            Debug.Log("Audio settings file not found, creating default");
            CreateDefaultSettings();
        }
    }

    public void SaveSettings()
    {
        Debug.Log("=== SAVING AUDIO SETTINGS ===");
        Debug.Log(
            $"Saving settings - Music: {currentSettings.musicVolume}, SFX: {currentSettings.sfxVolume}, Ambience: {currentSettings.ambienceVolume}"
        );

        try
        {
            string jsonData = JsonUtility.ToJson(currentSettings, true);
            Debug.Log("JSON to save: " + jsonData);
            Debug.Log("Save path: " + saveFilePath);

            File.WriteAllText(saveFilePath, jsonData);

            // Verify file was written
            if (File.Exists(saveFilePath))
            {
                string verifyData = File.ReadAllText(saveFilePath);
                Debug.Log("Verification - file saved with content: " + verifyData);
            }

            Debug.Log("Audio settings saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save audio settings: " + e.Message);
        }
    }

    void CreateDefaultSettings()
    {
        Debug.Log("=== CREATING DEFAULT SETTINGS ===");
        currentSettings = new AudioSettingsData();
        currentSettings.musicVolume = 1.0f;
        currentSettings.sfxVolume = 1.0f;
        currentSettings.ambienceVolume = 1.0f;
        Debug.Log("Default settings created - ALL volumes set to 1.0 (100%)");
        SaveSettings();
    }

    public void ApplySettingsToAudio()
    {
        Debug.Log("=== APPLYING SETTINGS TO AUDIO ===");

        // PROTEKSI: Pastikan BacksoundPlayer masih hidup
        EnsureBacksoundPlayerAlive();

        Debug.Log($"BacksoundPlayer.instance exists: {BacksoundPlayer.instance != null}");

        // UNIVERSAL APPROACH: Update ALL AudioSources di scene terlebih dahulu
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        Debug.Log($"[UNIVERSAL] Found {allAudioSources.Length} AudioSources in current scene");

        int musicUpdated = 0;
        int sfxUpdated = 0;

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            var audioSrc = allAudioSources[i];
            if (audioSrc != null)
            {
                // Simplified log without stack trace
                if (audioSrc.isPlaying || audioSrc.loop)
                {
                    Debug.Log(
                        $"[UNIVERSAL] AudioSource[{i}]: '{audioSrc.gameObject.name}' - volume: {audioSrc.volume}, isPlaying: {audioSrc.isPlaying}, loop: {audioSrc.loop}"
                    );
                }

                // Update music (biasanya loop=true, tapi bisa juga yang sedang playing)
                if (audioSrc.loop || (audioSrc.isPlaying && audioSrc.clip != null))
                {
                    audioSrc.volume = currentSettings.musicVolume;
                    musicUpdated++;
                    Debug.Log(
                        $"[UNIVERSAL] Updated MUSIC AudioSource '{audioSrc.gameObject.name}' to volume: {audioSrc.volume}"
                    );
                }
                // Update SFX (biasanya loop=false)
                else if (!audioSrc.loop)
                {
                    audioSrc.volume = currentSettings.sfxVolume;
                    sfxUpdated++;
                }
            }
        }

        Debug.Log(
            $"[UNIVERSAL] Updated {musicUpdated} music AudioSources, {sfxUpdated} SFX AudioSources"
        );

        // INSTANCE DEBUGGING - Cek apakah instance yang benar
        if (BacksoundPlayer.instance != null)
        {
            Debug.Log(
                $"[INSTANCE] BacksoundPlayer active: {BacksoundPlayer.instance.isActiveAndEnabled}"
            );

            var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                Debug.Log(
                    $"[INSTANCE] AudioSource - isPlaying: {audioSource.isPlaying}, volume: {audioSource.volume}, clip: {(audioSource.clip != null ? audioSource.clip.name : "NULL")}"
                );
            }
            else
            {
                Debug.LogError("[INSTANCE] AudioSource is NULL on BacksoundPlayer!");
            }

            BacksoundPlayer.instance.SetMusicVolume(currentSettings.musicVolume);
            BacksoundPlayer.instance.SetSFXVolume(currentSettings.sfxVolume);
            BacksoundPlayer.instance.SetAmbienceVolume(currentSettings.ambienceVolume);

            // EXTRA: Direct AudioSource update untuk memastikan
            if (audioSource != null)
            {
                audioSource.volume = currentSettings.musicVolume;
                Debug.Log($"[DIRECT] AudioSource volume updated to: {audioSource.volume}");
            }

            Debug.Log(
                $"Applied audio settings - Music: {currentSettings.musicVolume}, SFX: {currentSettings.sfxVolume}, Ambience: {currentSettings.ambienceVolume}"
            );
        }
        else
        {
            Debug.LogWarning("BacksoundPlayer.instance is NULL - starting coroutine to retry...");
            StartCoroutine(RetryApplySettingsWithDelay());
        }
    }

    // Retry mechanism jika BacksoundPlayer belum ready
    private System.Collections.IEnumerator RetryApplySettingsWithDelay()
    {
        Debug.Log("Retrying apply settings with delay...");

        int attempts = 0;
        while (BacksoundPlayer.instance == null && attempts < 20) // Increase attempts
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
            Debug.Log($"Retry attempt {attempts}: BacksoundPlayer still NULL");
        }

        if (BacksoundPlayer.instance != null)
        {
            BacksoundPlayer.instance.SetMusicVolume(currentSettings.musicVolume);
            BacksoundPlayer.instance.SetSFXVolume(currentSettings.sfxVolume);
            BacksoundPlayer.instance.SetAmbienceVolume(currentSettings.ambienceVolume);
            Debug.Log("RETRY SUCCESS - Audio settings applied after retry!");
        }
        else
        {
            Debug.LogError("RETRY FAILED - BacksoundPlayer still not found after 20 attempts");
        }
    }

    // Public methods untuk update settings TEMPORARY (tidak auto-save)
    public void SetMusicVolumeTemporary(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        Debug.Log($"[TEMP] Setting music volume to: {currentSettings.musicVolume}");

        // UNIVERSAL APPROACH: Update ALL AudioSources yang sedang playing music
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        int updatedCount = 0;

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            var audioSrc = allAudioSources[i];
            if (audioSrc != null && audioSrc.isPlaying && audioSrc.clip != null)
            {
                // Update volume untuk semua music AudioSource
                float oldVolume = audioSrc.volume;
                audioSrc.volume = currentSettings.musicVolume;
                updatedCount++;
                Debug.Log(
                    $"[TEMP-UNIVERSAL] Updated AudioSource '{audioSrc.gameObject.name}': {oldVolume} → {audioSrc.volume}"
                );
            }
        }

        Debug.Log($"[TEMP-UNIVERSAL] Updated {updatedCount} AudioSources total");

        // Backup: Try BacksoundPlayer instance juga
        if (BacksoundPlayer.instance != null)
        {
            BacksoundPlayer.instance.SetMusicVolume(currentSettings.musicVolume);
            Debug.Log(
                $"[TEMP] Applied music volume to BacksoundPlayer: {currentSettings.musicVolume}"
            );
        }
        else
        {
            Debug.LogWarning("[TEMP] BacksoundPlayer.instance is NULL!");
        }
        // Tidak save ke JSON - hanya temporary
    }

    public void SetSFXVolumeTemporary(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        Debug.Log($"[TEMP] Setting SFX volume to: {currentSettings.sfxVolume}");

        // UNIVERSAL APPROACH untuk SFX juga
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        int updatedCount = 0;

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            var audioSrc = allAudioSources[i];
            if (audioSrc != null && !audioSrc.loop) // SFX biasanya tidak loop
            {
                audioSrc.volume = currentSettings.sfxVolume;
                updatedCount++;
            }
        }

        Debug.Log($"[TEMP-UNIVERSAL] Updated {updatedCount} SFX AudioSources");

        if (BacksoundPlayer.instance != null)
        {
            BacksoundPlayer.instance.SetSFXVolume(currentSettings.sfxVolume);
            Debug.Log($"[TEMP] Applied SFX volume to BacksoundPlayer: {currentSettings.sfxVolume}");
        }
        else
        {
            Debug.LogWarning("[TEMP] BacksoundPlayer.instance is NULL!");
        }
        // Tidak save ke JSON - hanya temporary
    }

    public void SetAmbienceVolumeTemporary(float volume)
    {
        currentSettings.ambienceVolume = Mathf.Clamp01(volume);
        Debug.Log($"[TEMP] Setting ambience volume to: {currentSettings.ambienceVolume}");

        // UNIVERSAL APPROACH untuk Ambience
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        int updatedCount = 0;

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            var audioSrc = allAudioSources[i];
            if (audioSrc != null && audioSrc.isPlaying)
            {
                // Check jika ini ambience clip (bisa ditambah logic untuk detect ambience)
                if (audioSrc.clip != null && audioSrc.clip.name.ToLower().Contains("ambient"))
                {
                    audioSrc.volume = currentSettings.ambienceVolume;
                    updatedCount++;
                }
            }
        }

        Debug.Log($"[TEMP-UNIVERSAL] Updated {updatedCount} Ambience AudioSources");

        if (BacksoundPlayer.instance != null)
        {
            BacksoundPlayer.instance.SetAmbienceVolume(currentSettings.ambienceVolume);
            Debug.Log(
                $"[TEMP] Applied ambience volume to BacksoundPlayer: {currentSettings.ambienceVolume}"
            );
        }
        else
        {
            Debug.LogWarning("[TEMP] BacksoundPlayer.instance is NULL!");
        }
        // Tidak save ke JSON - hanya temporary
    }

    // Public methods untuk update settings PERMANENT (dengan save)
    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        if (BacksoundPlayer.instance != null)
            BacksoundPlayer.instance.SetMusicVolume(currentSettings.musicVolume);
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        if (BacksoundPlayer.instance != null)
            BacksoundPlayer.instance.SetSFXVolume(currentSettings.sfxVolume);
        SaveSettings();
    }

    public void SetAmbienceVolume(float volume)
    {
        currentSettings.ambienceVolume = Mathf.Clamp01(volume);
        if (BacksoundPlayer.instance != null)
            BacksoundPlayer.instance.SetAmbienceVolume(currentSettings.ambienceVolume);
        SaveSettings();
    }

    // OnEnable dipanggil setiap kali GameObject diaktifkan (termasuk saat scene transition)
    void OnEnable()
    {
        if (Instance == this && currentSettings != null)
        {
            Debug.Log("[OnEnable] AudioSettingsManager enabled, ensuring settings are applied");
            StartCoroutine(EnsureSettingsAppliedOnEnable());
        }
    }

    private System.Collections.IEnumerator EnsureSettingsAppliedOnEnable()
    {
        // Tunggu sebentar untuk memastikan semua komponen sudah ready
        yield return new WaitForSeconds(0.1f);

        // Cek dan pastikan volume BacksoundPlayer sesuai dengan settings
        if (BacksoundPlayer.instance != null)
        {
            var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                // Cek apakah volume masih sesuai dengan settings
                if (Mathf.Abs(audioSource.volume - currentSettings.musicVolume) > 0.01f)
                {
                    Debug.Log(
                        $"[OnEnable] Volume mismatch detected! AudioSource: {audioSource.volume}, Settings: {currentSettings.musicVolume}"
                    );
                    ApplySettingsToAudio();
                }
                else
                {
                    Debug.Log(
                        $"[OnEnable] Volume OK - AudioSource: {audioSource.volume}, Settings: {currentSettings.musicVolume}"
                    );
                }
            }
        }
    }

    // Method untuk memastikan BacksoundPlayer tetap hidup dan bermain musik
    public void EnsureBacksoundPlayerAlive()
    {
        Debug.Log("[PROTECTION] Checking BacksoundPlayer health...");

        if (BacksoundPlayer.instance == null)
        {
            Debug.LogError(
                "[PROTECTION] BacksoundPlayer instance is NULL! Creating emergency protection..."
            );

            // Cari BacksoundPlayer di scene
            BacksoundPlayer foundPlayer = FindObjectOfType<BacksoundPlayer>();
            if (foundPlayer != null)
            {
                Debug.Log(
                    "[PROTECTION] Found BacksoundPlayer in scene, ensuring DontDestroyOnLoad"
                );
                DontDestroyOnLoad(foundPlayer.gameObject);
            }
            else
            {
                Debug.LogError("[PROTECTION] No BacksoundPlayer found in scene!");
            }
        }
        else
        {
            var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                Debug.Log(
                    $"[PROTECTION] BacksoundPlayer OK - isPlaying: {audioSource.isPlaying}, volume: {audioSource.volume}"
                );

                // Pastikan volume sesuai dengan settings
                if (audioSource.volume != currentSettings.musicVolume)
                {
                    Debug.Log(
                        $"[PROTECTION] Correcting volume from {audioSource.volume} to {currentSettings.musicVolume}"
                    );
                    audioSource.volume = currentSettings.musicVolume;
                }
            }
        }
    }

    // PUBLIC DEBUG METHOD - bisa dipanggil dari luar untuk debug
    public void DebugCurrentState()
    {
        Debug.Log("=== AUDIO SETTINGS MANAGER DEBUG ===");
        Debug.Log($"Instance exists: {Instance != null}");
        Debug.Log(
            $"Current settings - Music: {currentSettings.musicVolume}, SFX: {currentSettings.sfxVolume}, Ambience: {currentSettings.ambienceVolume}"
        );
        Debug.Log($"BacksoundPlayer.instance exists: {BacksoundPlayer.instance != null}");

        if (BacksoundPlayer.instance != null)
        {
            var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                Debug.Log(
                    $"BacksoundPlayer AudioSource - volume: {audioSource.volume}, isPlaying: {audioSource.isPlaying}, enabled: {audioSource.enabled}"
                );
                Debug.Log(
                    $"BacksoundPlayer GameObject active: {BacksoundPlayer.instance.gameObject.activeInHierarchy}"
                );
            }
            else
            {
                Debug.LogError("BacksoundPlayer has no AudioSource component!");
            }
        }

        // List semua AudioSources di scene
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        Debug.Log($"Total AudioSources in scene: {allAudioSources.Length}");
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            var audioSrc = allAudioSources[i];
            Debug.Log(
                $"  [{i}] {audioSrc.gameObject.name} - volume: {audioSrc.volume}, isPlaying: {audioSrc.isPlaying}"
            );
        }
    }
}
