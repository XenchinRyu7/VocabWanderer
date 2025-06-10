using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TimerLine : MonoBehaviour
{
    private float duration;
    private float timeRemaining;
    private float originalWidth;
    private RectTransform rectTransform;

    public System.Action OnLineTimeout;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalWidth = 100;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            float ratio = timeRemaining / duration;
            rectTransform.sizeDelta = new Vector2(originalWidth * ratio, rectTransform.sizeDelta.y);
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                rectTransform.sizeDelta = new Vector2(0, rectTransform.sizeDelta.y);
                if (OnLineTimeout != null)
                    OnLineTimeout.Invoke();
            }
        }
    }

    public void StartLine(float newDuration)
    {
        Debug.Log($"Starting timer line with duration: {newDuration} seconds");
        duration = newDuration;
        timeRemaining = duration;
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(originalWidth, rectTransform.sizeDelta.y);
    }
}
