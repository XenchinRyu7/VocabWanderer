using UnityEngine;
using System.Collections;

/// <summary>
/// Script khusus untuk Settings scene - memastikan audio tetap hidup saat masuk ke Settings
/// Letakkan script ini di GameObject kosong di Settings scene
/// </summary>
public class SettingsSceneAudioProtector : MonoBehaviour
{
    [Header("Debug")]
    public bool enableDetailedLogs = true;

    void Start()
    {
        if (enableDetailedLogs)
        {
            Debug.Log("[SETTINGS-PROTECTOR] Settings scene started, checking audio status...");
        }

        StartCoroutine(ProtectAudioOnSceneStart());
    }

    private IEnumerator ProtectAudioOnSceneStart()
    {
        // Tunggu sebentar untuk memastikan semua sistem sudah ready
        yield return new WaitForSeconds(0.1f);

        if (enableDetailedLogs)
        {
            Debug.Log("[SETTINGS-PROTECTOR] Ensuring AudioSettingsManager exists...");
        }

        // Pastikan AudioSettingsManager ada
        if (AudioSettingsManager.Instance == null)
        {
            Debug.LogWarning("[SETTINGS-PROTECTOR] AudioSettingsManager not found! Creating emergency instance...");
            
            // Cari di scene apakah ada AudioSettingsManager
            AudioSettingsManager manager = FindObjectOfType<AudioSettingsManager>();
            if (manager == null)
            {
                // Buat emergency GameObject untuk AudioSettingsManager
                GameObject audioManagerGO = new GameObject("AudioSettingsManager_Emergency");
                manager = audioManagerGO.AddComponent<AudioSettingsManager>();
                Debug.Log("[SETTINGS-PROTECTOR] Created emergency AudioSettingsManager");
            }
        }

        // Tunggu sebentar lagi
        yield return new WaitForSeconds(0.1f);

        if (enableDetailedLogs)
        {
            Debug.Log("[SETTINGS-PROTECTOR] Checking BacksoundPlayer status...");
        }

        // Cek status BacksoundPlayer
        if (BacksoundPlayer.instance == null)
        {
            Debug.LogError("[SETTINGS-PROTECTOR] BacksoundPlayer instance is NULL! Searching for BacksoundPlayer in scene...");
            
            // Cari BacksoundPlayer di scene
            BacksoundPlayer[] allPlayers = FindObjectsOfType<BacksoundPlayer>();
            if (allPlayers.Length > 0)
            {
                Debug.Log($"[SETTINGS-PROTECTOR] Found {allPlayers.Length} BacksoundPlayer(s) in scene");
                for (int i = 0; i < allPlayers.Length; i++)
                {
                    Debug.Log($"[SETTINGS-PROTECTOR] BacksoundPlayer[{i}]: {allPlayers[i].gameObject.name}");
                    
                    // Pastikan DontDestroyOnLoad
                    DontDestroyOnLoad(allPlayers[i].gameObject);
                }
            }
            else
            {
                Debug.LogError("[SETTINGS-PROTECTOR] No BacksoundPlayer found in scene! Audio will not work properly.");
            }
        }
        else
        {
            var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                if (enableDetailedLogs)
                {
                    Debug.Log($"[SETTINGS-PROTECTOR] BacksoundPlayer OK - isPlaying: {audioSource.isPlaying}, volume: {audioSource.volume}");
                }

                // Jika musik tidak bermain, coba restart
                if (!audioSource.isPlaying && audioSource.clip != null)
                {
                    Debug.Log("[SETTINGS-PROTECTOR] Music not playing, attempting to restart...");
                    audioSource.Play();
                }
            }
        }

        // Tunggu sebentar dan cek ulang
        yield return new WaitForSeconds(0.2f);

        // Force apply settings untuk memastikan volume benar
        if (AudioSettingsManager.Instance != null)
        {
            Debug.Log("[SETTINGS-PROTECTOR] Force applying audio settings...");
            AudioSettingsManager.Instance.ApplySettingsToAudio();
            
            if (enableDetailedLogs)
            {
                // Debug state final
                AudioSettingsManager.Instance.DebugCurrentState();
            }
        }

        if (enableDetailedLogs)
        {
            Debug.Log("[SETTINGS-PROTECTOR] Audio protection sequence completed");
        }
    }

    // Method yang bisa dipanggil manual untuk debugging
    [ContextMenu("Force Audio Protection")]
    public void ForceAudioProtection()
    {
        StartCoroutine(ProtectAudioOnSceneStart());
    }

    // Method untuk test apakah audio sistem bekerja
    [ContextMenu("Test Audio System")]
    public void TestAudioSystem()
    {
        Debug.Log("=== TESTING AUDIO SYSTEM ===");
        
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance.DebugCurrentState();
        }
        else
        {
            Debug.LogError("AudioSettingsManager.Instance is NULL!");
        }

        if (BacksoundPlayer.instance != null)
        {
            var audioSource = BacksoundPlayer.instance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                Debug.Log($"BacksoundPlayer Test - isPlaying: {audioSource.isPlaying}, volume: {audioSource.volume}, clip: {(audioSource.clip != null ? audioSource.clip.name : "NULL")}");
            }
        }
        else
        {
            Debug.LogError("BacksoundPlayer.instance is NULL!");
        }
    }
}
