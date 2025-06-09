using UnityEngine;
using System.Collections.Generic;

public class AnswerChecker : MonoBehaviour
{
    public static AnswerChecker Instance;
    public Transform letterSlotParent;

    void Awake() { Instance = this; }

    public void SubmitAnswer() {
        var slots = letterSlotParent.GetComponentsInChildren<LetterDropSlot>();
        string userAnswer = "";
        foreach (var slot in slots) {
            char? c = slot.GetCurrentLetter();
            userAnswer += c.HasValue ? c.Value.ToString() : "_";
        }
        bool correct = userAnswer.Replace("_","") == GameManager.Instance.currentQuestion.answer;
        GameManager.Instance.OnAnswerSubmitted(correct);
    }

    public void ClearSlots() {
        var slots = letterSlotParent.GetComponentsInChildren<LetterDropSlot>();
        foreach (var slot in slots) slot.ClearSlot();
    }
}
