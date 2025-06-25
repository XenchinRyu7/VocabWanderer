using UnityEngine;

public class CrossSceneAudioHelper : MonoBehaviour
{
    [Header("Debug")]
    public bool enableDebugLogs = true;

    public void PlayButtonClickSound()
    {
        if (enableDebugLogs)
        {
            Debug.Log(
                $"[CrossSceneAudioHelper] PlayButtonClickSound called from scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}"
            );
        }

        BacksoundPlayer.PlayButtonClickFromAnyScene();
    }

    [ContextMenu("Check BacksoundPlayer")]
    public void CheckBacksoundPlayer()
    {
        Debug.Log("=== CROSS SCENE AUDIO CHECK ===");
        Debug.Log(
            $"Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}"
        );
        Debug.Log($"BacksoundPlayer.instance exists: {BacksoundPlayer.instance != null}");

        if (BacksoundPlayer.instance != null)
        {
            Debug.Log($"BacksoundPlayer GameObject: {BacksoundPlayer.instance.gameObject.name}");
            Debug.Log(
                $"BacksoundPlayer active: {BacksoundPlayer.instance.gameObject.activeInHierarchy}"
            );

            Debug.Log("Testing button click sound...");
            BacksoundPlayer.PlayButtonClickFromAnyScene();
        }
        else
        {
            Debug.LogError("BacksoundPlayer instance not found!");
        }
    }

    public void PlayButtonClickSoundDirect()
    {
        if (BacksoundPlayer.instance != null)
        {
            BacksoundPlayer.instance.PlayButtonClickSound();
        }
        else
        {
            Debug.LogError("[CrossSceneAudioHelper] BacksoundPlayer.instance is null!");
        }
    }
}
