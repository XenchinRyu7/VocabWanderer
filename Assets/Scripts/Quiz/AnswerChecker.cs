using UnityEngine;
using System.Collections.Generic;

public class AnswerChecker : MonoBehaviour
{
    public static AnswerChecker Instance;
    public Transform letterSlotParent;

    void Awake() { Instance = this; }

    public void SubmitAnswer() {
        var slots = letterSlotParent.GetComponentsInChildren<LetterDropSlot>();
        List<string> userInput = new List<string>();
        foreach (var slot in slots) {
            // Ambil text dari setiap slot input user (tanpa cek enabled)
            if (slot.displayText != null) {
                var text = slot.displayText.text;
                userInput.Add(string.IsNullOrEmpty(text) ? "_" : text.ToLower());
            }
        }
        string userAnswer = string.Join("", userInput).Replace("_", "");
        string kunci = string.Join("", GameManager.Instance.currentQuestion.missing_letters).ToLower();
        Debug.Log($"[AnswerChecker] userAnswer='{userAnswer}', kunci='{kunci}'");
        bool correct = userAnswer == kunci;
        Debug.Log($"[AnswerChecker] correct={correct}");
        if (correct)
        {
            AddHeart();
        }
        GameManager.Instance.OnAnswerSubmitted(correct);
    }

    public void ClearSlots() {
        var slots = letterSlotParent.GetComponentsInChildren<LetterDropSlot>();
        foreach (var slot in slots) slot.ClearSlot();
    }

    // Fungsi untuk menambah heart, maksimal 5
    public void AddHeart()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.health < 5)
            {
                GameManager.Instance.health++;
                DynamicUIBuilder.Instance.UpdateHealthUI(GameManager.Instance.health);
            }
        }
    }
}
