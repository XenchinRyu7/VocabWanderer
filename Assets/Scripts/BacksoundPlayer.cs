using UnityEngine;
using UnityEngine.SceneManagement;

public class BacksoundPlayer : MonoBehaviour
{
    public static BacksoundPlayer instance;

    public AudioClip startClip;
    public AudioClip quizClip;
    public AudioClip timeAlmostOutClip;
    public AudioClip correctClip;
    public AudioClip wrongClip;
    public AudioClip gameOverClip;
    public AudioClip successClip;
    public AudioClip buttonClickClip;
    private AudioSource audioSource;
    private AudioSource effectAudioSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Range(0f, 1f)]
    public float ambienceVolume = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("BacksoundPlayer duplicate detected, destroying new instance.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on BacksoundPlayer GameObject!");
        } // Setup AudioSource kedua untuk sound effects
        effectAudioSource = gameObject.AddComponent<AudioSource>();
        effectAudioSource.loop = false;
        effectAudioSource.volume = sfxVolume;
        effectAudioSource.playOnAwake = false;

        audioSource.loop = true;
        audioSource.volume = musicVolume;
        audioSource.clip = startClip;
        audioSource.Play();

        // Initialize AudioSettingsManager jika belum ada
        InitializeAudioSettings();

        Debug.Log(
            "BacksoundPlayer initialized with startClip: "
                + (startClip != null ? startClip.name : "NULL")
        );
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("BacksoundPlayer OnEnable - sceneLoaded event subscribed.");
    }

    private void OnDisable()
    {
        Debug.Log("BacksoundPlayer OnDisable dipanggil! (kemungkinan coroutine stop)");
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("BacksoundPlayer OnDisable - sceneLoaded event unsubscribed.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        // REFRESH effectAudioSource setelah scene change untuk fix sound effect issue
        RefreshEffectAudioSource();
        Debug.Log("[SCENE_LOADED] effectAudioSource refreshed");

        if (scene.name == "QuestionScene")
        {
            Debug.Log("Changing to quizClip: " + (quizClip != null ? quizClip.name : "NULL"));
            ChangeBacksound(quizClip);
        }
        else if (scene.name == "MainMenu")
        {
            Debug.Log("Changing to startClip: " + (startClip != null ? startClip.name : "NULL"));
            ChangeBacksound(startClip);
        }
    }

    public void ChangeBacksound(AudioClip newClip)
    {
        if (newClip == null)
        {
            Debug.LogError("ChangeBacksound called with null AudioClip!");
            return;
        }
        if (audioSource.clip == newClip)
        {
            Debug.Log("Backsound is already playing: " + newClip.name);
            return;
        }

        Debug.Log(
            "Backsound changing from "
                + (audioSource.clip != null ? audioSource.clip.name : "NULL")
                + " to "
                + newClip.name
        );
        StartCoroutine(FadeToNewClip(newClip));
    }

    private System.Collections.IEnumerator FadeToNewClip(AudioClip newClip)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null in FadeToNewClip!");
            Debug.LogWarning("Coroutine FadeToNewClip berhenti karena audioSource null");
            yield break;
        }

        if (newClip == null)
        {
            Debug.LogError("FadeToNewClip called with null AudioClip!");
            Debug.LogWarning("Coroutine FadeToNewClip berhenti karena newClip null");
            yield break;
        }

        float t = 0f;
        float duration = 1f;
        float startVolume = audioSource.volume;

        // Tentukan target volume berdasarkan jenis clip
        float targetVolume = musicVolume; // Default music
        if (newClip == timeAlmostOutClip || newClip == gameOverClip || newClip == successClip)
        {
            targetVolume = ambienceVolume; // Gunakan ambience volume
        }

        Debug.Log(
            $"[FADE] Target volume determined: {targetVolume} (musicVolume: {musicVolume}, ambienceVolume: {ambienceVolume})"
        );

        Debug.Log("Fading out current clip...");

        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;

        Debug.Log("Clip changed to: " + newClip.name + ", starting playback...");

        audioSource.Play();

        Debug.Log(
            $"audioSource.Play() dipanggil. Status: isPlaying={audioSource.isPlaying}, clip={(audioSource.clip != null ? audioSource.clip.name : "NULL")}, volume={audioSource.volume}, mute={audioSource.mute}"
        );
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        Debug.Log("Fade-in complete. Volume set to: " + targetVolume);
    }

    private void PlaySoundEffect(AudioClip effectClip)
    {
        // SAFETY CHECK: Ensure effectAudioSource is ready
        if (effectAudioSource == null)
        {
            Debug.LogError("[SFX] effectAudioSource is null, attempting to recreate...");
            RefreshEffectAudioSource();
            if (effectAudioSource == null)
            {
                Debug.LogError("[SFX] Failed to recreate effectAudioSource!");
                return;
            }
        }

        if (effectClip == null)
        {
            Debug.LogError("[SFX] effectClip is null!");
            return;
        }

        // FORCE refresh settings setiap kali play
        effectAudioSource.volume = sfxVolume;
        effectAudioSource.enabled = true;

        Debug.Log($"[SFX] Playing sound effect: {effectClip.name}");
        Debug.Log(
            $"[SFX] effectAudioSource status - enabled: {effectAudioSource.enabled}, volume: {effectAudioSource.volume}"
        );
        Debug.Log($"[SFX] sfxVolume field: {sfxVolume}");

        effectAudioSource.Stop(); // Stop effect sebelumnya jika ada
        effectAudioSource.clip = effectClip;
        effectAudioSource.Play();

        Debug.Log(
            $"[SFX] After Play() - isPlaying: {effectAudioSource.isPlaying}, clip: {effectAudioSource.clip?.name}"
        );

        // EXTRA DEBUG: Check if audio is actually audible
        if (!effectAudioSource.isPlaying)
        {
            Debug.LogError("[SFX] WARNING: effectAudioSource.isPlaying is FALSE after Play()!");
        }
        if (effectAudioSource.volume <= 0)
        {
            Debug.LogError(
                $"[SFX] WARNING: effectAudioSource.volume is {effectAudioSource.volume}!"
            );
        }
    }

    public void PlayTimeAlmostOutSound()
    {
        if (timeAlmostOutClip == null)
        {
            Debug.LogError("PlayTimeAlmostOutSound called but timeAlmostOutClip is null!");
            return;
        }
        Debug.Log("Playing timeAlmostOutClip: " + timeAlmostOutClip.name);
        StartCoroutine(FadeToNewClip(timeAlmostOutClip));
    }

    public void PlayQuizBacksound()
    {
        if (quizClip == null)
        {
            Debug.LogError("PlayQuizBacksound called but quizClip is null!");
            return;
        }
        Debug.Log("Playing quizClip: " + quizClip.name);
        StartCoroutine(FadeToNewClip(quizClip));
    }

    public void PlayStartBacksound()
    {
        if (startClip == null)
        {
            Debug.LogError("PlayStartBacksound called but startClip is null!");
            return;
        }
        Debug.Log("Playing startClip: " + startClip.name);
        StartCoroutine(FadeToNewClip(startClip));
    }

    public void PlayCorrectSound()
    {
        if (correctClip == null)
        {
            Debug.LogError("PlayCorrectSound called but correctClip is null!");
            return;
        }
        Debug.Log("Playing correctClip: " + correctClip.name);
        PlaySoundEffect(correctClip);
    }

    public void PlayWrongSound()
    {
        if (wrongClip == null)
        {
            Debug.LogError("PlayWrongSound called but wrongClip is null!");
            return;
        }
        Debug.Log("Playing wrongClip: " + wrongClip.name);
        PlaySoundEffect(wrongClip);
    }

    public void PlayGameOverSound()
    {
        if (gameOverClip == null)
        {
            Debug.LogError("PlayGameOverSound called but gameOverClip is null!");
            return;
        }
        Debug.Log("Playing gameOverClip: " + gameOverClip.name);
        StartCoroutine(FadeToNewClip(gameOverClip));
    }

    public void PlaySuccessSound()
    {
        if (successClip == null)
        {
            Debug.LogError("PlaySuccessSound called but successClip is null!");
            return;
        }
        Debug.Log("Playing successClip: " + successClip.name);
        StartCoroutine(FadeToNewClip(successClip));
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickClip == null)
        {
            Debug.LogError("PlayButtonClickSound called but buttonClickClip is null!");
            return;
        }

        Debug.Log(
            $"[BUTTON] PlayButtonClickSound called - BacksoundPlayer instance: {instance != null}"
        );
        Debug.Log($"[BUTTON] BacksoundPlayer GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"[BUTTON] buttonClickClip: {buttonClickClip.name}");

        PlaySoundEffect(buttonClickClip);
    }

    // Contoh fungsi untuk dihubungkan ke button di UI
    public void OnClickPlayStartBacksound()
    {
        PlayStartBacksound();
    }

    public void OnClickPlayQuizBacksound()
    {
        PlayQuizBacksound();
    }

    public void OnClickPlayTimeAlmostOutSound()
    {
        PlayTimeAlmostOutSound();
    }

    public void OnClickPlayCorrectSound()
    {
        PlayCorrectSound();
    }

    public void OnClickPlayWrongSound()
    {
        PlayWrongSound();
    }

    public void OnClickPlayGameOverSound()
    {
        PlayGameOverSound();
    }

    public void OnClickPlaySuccessSound()
    {
        PlaySuccessSound();
    }

    // Fungsi untuk toggle play/pause backsound dari button
    public void OnClickToggleBacksound()
    {
        if (audioSource == null)
            return;
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Backsound paused by button click.");
        }
        else
        {
            audioSource.UnPause();
            Debug.Log("Backsound resumed by button click.");
        }
    }

    // Fungsi untuk play/pause backsound saat button di-click (bukan fade, hanya toggle play/pause)
    public void OnButtonClickBacksound()
    {
        if (audioSource == null)
            return;
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Backsound paused by OnButtonClickBacksound.");
        }
        else
        {
            audioSource.UnPause();
            Debug.Log("Backsound resumed by OnButtonClickBacksound.");
        }
    }

    // ===== VOLUME CONTROL METHODS =====

    public void SetMusicVolume(float volume)
    {
        // UPDATE field musicVolume juga!
        musicVolume = Mathf.Clamp01(volume);
        Debug.Log($"[DEBUG] Updated musicVolume field to: {musicVolume}");
        Debug.Log($"[DEBUG] AudioSource exists: {audioSource != null}");
        if (audioSource != null)
        {
            Debug.Log($"[DEBUG] AudioSource isPlaying: {audioSource.isPlaying}");
            Debug.Log(
                $"[DEBUG] AudioSource clip: {(audioSource.clip != null ? audioSource.clip.name : "NULL")}"
            );
            Debug.Log($"[DEBUG] AudioSource volume BEFORE: {audioSource.volume}");

            audioSource.volume = musicVolume;

            Debug.Log($"[DEBUG] AudioSource volume AFTER: {audioSource.volume}");
        }
        else
        {
            Debug.LogError("[DEBUG] AudioSource is NULL!");
        }
        Debug.Log($"Music volume set to: {musicVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        // UPDATE field sfxVolume juga!
        sfxVolume = Mathf.Clamp01(volume);
        if (effectAudioSource != null)
        {
            effectAudioSource.volume = sfxVolume;
        }
        Debug.Log($"SFX volume set to: {sfxVolume}");
    }

    public void SetAmbienceVolume(float volume)
    {
        // UPDATE field ambienceVolume juga!
        ambienceVolume = Mathf.Clamp01(volume);
        // Ambience menggunakan audioSource utama seperti music, tapi saat fade
        // Volume akan diterapkan saat FadeToNewClip
        Debug.Log($"Ambience volume set to: {ambienceVolume}");
    }

    // Method untuk mengupdate volume saat ini sesuai kategori
    public void ApplyCurrentVolume()
    {
        if (audioSource != null)
            audioSource.volume = musicVolume;
        if (effectAudioSource != null)
            effectAudioSource.volume = sfxVolume;
    }

    // Initialize AudioSettingsManager dan load settings
    private void InitializeAudioSettings()
    {
        // SAFETY: Hanya singleton BacksoundPlayer yang boleh initialize AudioSettingsManager
        if (instance != this)
        {
            Debug.Log("InitializeAudioSettings called by non-singleton BacksoundPlayer, skipping");
            return;
        }

        // Cek apakah AudioSettingsManager sudah ada
        if (AudioSettingsManager.Instance == null)
        {
            // Buat AudioSettingsManager GameObject jika belum ada
            GameObject audioSettingsGO = new GameObject("AudioSettingsManager");
            audioSettingsGO.AddComponent<AudioSettingsManager>();
            Debug.Log("AudioSettingsManager created by BacksoundPlayer singleton");
        }
        else
        {
            // Jika sudah ada, apply settings langsung
            AudioSettingsManager.Instance.ApplySettingsToAudio();
            Debug.Log("AudioSettingsManager already exists, applied settings");
        }
    }

    // ===== DEBUG & SCENE ACCESS METHODS =====

    [ContextMenu("Debug BacksoundPlayer State")]
    public void DebugBacksoundPlayerState()
    {
        Debug.Log("=== BACKSOUNDPLAYER DEBUG STATE ===");
        Debug.Log($"Instance exists: {instance != null}");
        Debug.Log($"This is singleton: {instance == this}");
        Debug.Log($"GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"GameObject name: {gameObject.name}");
        Debug.Log($"Main AudioSource: {audioSource != null}");
        Debug.Log($"Effect AudioSource: {effectAudioSource != null}");
        if (audioSource != null)
        {
            Debug.Log(
                $"Main AudioSource - isPlaying: {audioSource.isPlaying}, volume: {audioSource.volume}"
            );
        }

        if (effectAudioSource != null)
        {
            Debug.Log(
                $"Effect AudioSource - enabled: {effectAudioSource.enabled}, volume: {effectAudioSource.volume}"
            );
        }
        Debug.Log(
            $"Button click clip: {(buttonClickClip != null ? buttonClickClip.name : "NULL")}"
        );
        Debug.Log("=== END DEBUG ===");
    }

    [ContextMenu("Test Button Click Sound")]
    public void TestButtonClickSound()
    {
        Debug.Log("[TEST] Manual button click sound test triggered");
        PlayButtonClickSound();
    }

    // Static method untuk akses dari scene lain
    public static void PlayButtonClickFromAnyScene()
    {
        if (instance != null)
        {
            Debug.Log("[STATIC] Playing button click from any scene");
            instance.PlayButtonClickSound();
        }
        else
        {
            Debug.LogError(
                "[STATIC] BacksoundPlayer instance not found! Cannot play button sound."
            );
        }
    } // Method untuk re-initialize effectAudioSource jika rusak

    public void RefreshEffectAudioSource()
    {
        if (effectAudioSource == null)
        {
            Debug.Log("[REFRESH] effectAudioSource is null, recreating...");
            effectAudioSource = gameObject.AddComponent<AudioSource>();
            effectAudioSource.loop = false;
            effectAudioSource.volume = sfxVolume;
            effectAudioSource.playOnAwake = false;
            Debug.Log("[REFRESH] effectAudioSource recreated successfully");
        }
        else
        {
            Debug.Log("[REFRESH] effectAudioSource exists, refreshing settings...");
            effectAudioSource.volume = sfxVolume;
            effectAudioSource.enabled = true;

            // Ensure effectAudioSource is properly configured
            if (!effectAudioSource.enabled)
            {
                effectAudioSource.enabled = true;
                Debug.Log("[REFRESH] effectAudioSource was disabled, re-enabled");
            }
        }

        Debug.Log(
            $"[REFRESH] Final state - enabled: {effectAudioSource.enabled}, volume: {effectAudioSource.volume}"
        );
    }

    [ContextMenu("Refresh Effect Audio Source")]
    public void RefreshEffectAudioSourceManual()
    {
        RefreshEffectAudioSource();
    }

    [ContextMenu("Force Audio Test")]
    public void ForceAudioTest()
    {
        Debug.Log("[FORCE_TEST] Starting forced audio test...");

        // Test dengan audio clip sederhana
        if (buttonClickClip != null)
        {
            Debug.Log("[FORCE_TEST] Playing buttonClickClip directly...");

            // Method 1: Direct AudioSource.PlayOneShot
            if (effectAudioSource != null)
            {
                effectAudioSource.PlayOneShot(buttonClickClip, sfxVolume);
                Debug.Log("[FORCE_TEST] Method 1: PlayOneShot executed");
            }

            // Method 2: Create temporary AudioSource
            GameObject tempGO = new GameObject("TempAudioTest");
            AudioSource tempAudio = tempGO.AddComponent<AudioSource>();
            tempAudio.clip = buttonClickClip;
            tempAudio.volume = sfxVolume;
            tempAudio.Play();
            Debug.Log("[FORCE_TEST] Method 2: Temporary AudioSource created and played");

            // Destroy temp object after 2 seconds
            Destroy(tempGO, 2f);
        }
        else
        {
            Debug.LogError("[FORCE_TEST] buttonClickClip is null!");
        }
    }

    // Method alternatif menggunakan PlayOneShot
    public void PlayButtonClickSoundAlt()
    {
        if (buttonClickClip == null)
        {
            Debug.LogError("PlayButtonClickSoundAlt called but buttonClickClip is null!");
            return;
        }

        if (effectAudioSource == null)
        {
            Debug.LogError("[ALT] effectAudioSource is null!");
            RefreshEffectAudioSource();
            return;
        }

        Debug.Log($"[ALT] Playing button click with PlayOneShot - volume: {sfxVolume}");
        effectAudioSource.PlayOneShot(buttonClickClip, sfxVolume);
        Debug.Log("[ALT] PlayOneShot executed");
    }
}
