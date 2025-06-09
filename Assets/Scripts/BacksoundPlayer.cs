using UnityEngine;
using UnityEngine.SceneManagement;

public class BacksoundPlayer : MonoBehaviour
{
    public static BacksoundPlayer instance;

    public AudioClip startClip;
    public AudioClip quizClip;
    private AudioSource audioSource;

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
        }
        audioSource.loop = true;
        audioSource.volume = 1f; // Pastikan volume tidak 0
        audioSource.clip = startClip;
        audioSource.Play();

        Debug.Log("BacksoundPlayer initialized with startClip: " + (startClip != null ? startClip.name : "NULL"));
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

        Debug.Log("Backsound changing from " + (audioSource.clip != null ? audioSource.clip.name : "NULL") + " to " + newClip.name);
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

        Debug.Log($"audioSource.Play() dipanggil. Status: isPlaying={audioSource.isPlaying}, clip={(audioSource.clip != null ? audioSource.clip.name : "NULL")}, volume={audioSource.volume}, mute={audioSource.mute}");

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / duration);
            yield return null;
        }

        audioSource.volume = startVolume;
        Debug.Log("Fade-in complete. Volume restored to: " + startVolume);
    }
}
