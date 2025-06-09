using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance;
    public TextMeshProUGUI timerText;
    private float timeLeft;
    private bool running = false;

    void Awake() { Instance = this; }

    public void StartTimer(int seconds)
    {
        timeLeft = seconds;
        running = true;
        UpdateTimerText();
    }

    void Update()
    {
        if (!running) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            running = false;
            timeLeft = 0;
            UpdateTimerText();
            GameManager.Instance.OnTimeOut();
        }
        else
        {
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    public void StopTimer() { running = false; }
}
