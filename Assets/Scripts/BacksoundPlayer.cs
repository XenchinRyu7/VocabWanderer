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
    public AudioClip buttonClickClip; // Variabel untuk menyimpan clip suara klik tombol
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
        audioSource.volume = 1f; 
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
        StartCoroutine(FadeToNewClip(correctClip));
    }

    public void PlayWrongSound()
    {
        if (wrongClip == null)
        {
            Debug.LogError("PlayWrongSound called but wrongClip is null!");
            return;
        }
        Debug.Log("Playing wrongClip: " + wrongClip.name);
        StartCoroutine(FadeToNewClip(wrongClip));
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
        Debug.Log("Playing buttonClickClip: " + buttonClickClip.name);
        audioSource.PlayOneShot(buttonClickClip);
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
        if (audioSource == null) return;
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
        if (audioSource == null) return;
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
}
