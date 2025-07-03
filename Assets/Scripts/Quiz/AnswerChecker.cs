using System.Collections.Generic;
using UnityEngine;

public class AnswerChecker : MonoBehaviour
{
    public static AnswerChecker Instance;
    public Transform letterSlotParent;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("[AnswerChecker] Replacing old instance with new scene instance");
        }

        Instance = this;
        Debug.Log("[AnswerChecker] Fresh instance created for this scene");

        // Validate letterSlotParent
        if (letterSlotParent == null)
        {
            Debug.LogError("[AnswerChecker] letterSlotParent is null! Please assign in inspector.");
        }
    }

    public void SubmitAnswer()
    {
        if (letterSlotParent == null)
        {
            Debug.LogError("[AnswerChecker] Cannot submit answer: letterSlotParent is null!");
            return;
        }

        if (!letterSlotParent.gameObject.activeInHierarchy)
        {
            Debug.LogError("[AnswerChecker] Cannot submit answer: letterSlotParent is not active!");
            return;
        }

        Debug.Log("[AnswerChecker] SubmitAnswer called with valid letterSlotParent");
        var slots = letterSlotParent.GetComponentsInChildren<LetterDropSlot>();
        List<string> userInput = new List<string>();
        foreach (var slot in slots)
        {
            if (slot.displayText != null)
            {
                var text = slot.displayText.text;
                userInput.Add(string.IsNullOrEmpty(text) ? "_" : text.ToLower());
            }
        }
        string userAnswer = string.Join("", userInput).Replace("_", "");
        string kunci = string.Join("", GameManager.Instance.currentQuestion.missing_letters)
            .ToLower();
        Debug.Log($"[AnswerChecker] userAnswer='{userAnswer}', kunci='{kunci}'");
        bool correct = userAnswer == kunci;
        Debug.Log($"[AnswerChecker] correct={correct}");
        if (correct)
        {
            AddHeart();
        }
        GameManager.Instance.OnAnswerSubmitted(correct);
    }

    public void ClearSlots()
    {
        var slots = letterSlotParent.GetComponentsInChildren<LetterDropSlot>();
        foreach (var slot in slots)
            slot.ClearSlot();
    }

    public void AddHeart()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.health < 5)
            {
                GameManager.Instance.health++;
                DynamicUIBuilder dynamicUIBuilder = FindObjectOfType<DynamicUIBuilder>();
                if (dynamicUIBuilder != null)
                {
                    dynamicUIBuilder.UpdateHealthUI(GameManager.Instance.health);
                }
            }
        }
    }
}
