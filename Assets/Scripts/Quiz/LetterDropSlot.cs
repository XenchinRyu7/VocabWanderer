using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterDropSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public TextMeshProUGUI displayText;
    private char? currentLetter = null;

    public Sprite filledSprite;
    private UnityEngine.UI.Image slotImage;

    void Awake()
    {
        slotImage = GetComponent<UnityEngine.UI.Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetter dragged = eventData.pointerDrag.GetComponent<DraggableLetter>();
        if (dragged != null && !currentLetter.HasValue)
        {
            currentLetter = dragged.letter;
            displayText.text = currentLetter.ToString().ToUpper();
            Destroy(dragged.gameObject);
            if (filledSprite != null && slotImage != null)
                slotImage.sprite = filledSprite;
            this.enabled = false;
            var allSlots = transform.parent.GetComponentsInChildren<LetterDropSlot>();
            bool allFilled = true;
            foreach (var slot in allSlots)
            {
                if (!slot.GetCurrentLetter().HasValue)
                {
                    allFilled = false;
                    break;
                }
            }
            Debug.Log(
                $"[LetterDropSlot] allFilled={allFilled}, AnswerChecker.Instance null? {AnswerChecker.Instance == null}"
            );
            if (allFilled && AnswerChecker.Instance != null)
            {
                Debug.Log("[LetterDropSlot] Semua slot terisi, submit jawaban otomatis!");
                AnswerChecker.Instance.SubmitAnswer();
            }
        }
    }

    public char? GetCurrentLetter() => currentLetter;

    public void ClearSlot()
    {
        currentLetter = null;
        displayText.text = "";
    }
}
