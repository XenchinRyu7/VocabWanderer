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
            // Debug: cek semua AnswerChecker yang ada
            AnswerChecker[] allAnswerCheckers = FindObjectsOfType<AnswerChecker>();
            Debug.Log($"[LetterDropSlot] Total AnswerChecker found in scene: {allAnswerCheckers.Length}");
            
            for (int i = 0; i < allAnswerCheckers.Length; i++)
            {
                var checker = allAnswerCheckers[i];
                bool hasLetterSlotParent = checker.letterSlotParent != null;
                bool isActive = hasLetterSlotParent && checker.letterSlotParent.gameObject.activeInHierarchy;
                Debug.Log($"[LetterDropSlot] AnswerChecker[{i}]: hasLetterSlotParent={hasLetterSlotParent}, isActive={isActive}, name={checker.gameObject.name}");
            }
            
            // Debug: cek AnswerChecker.Instance
            if (AnswerChecker.Instance != null)
            {
                bool instanceHasParent = AnswerChecker.Instance.letterSlotParent != null;
                bool instanceIsActive = instanceHasParent && AnswerChecker.Instance.letterSlotParent.gameObject.activeInHierarchy;
                Debug.Log($"[LetterDropSlot] AnswerChecker.Instance: hasLetterSlotParent={instanceHasParent}, isActive={instanceIsActive}, name={AnswerChecker.Instance.gameObject.name}");
            }
            else
            {
                Debug.Log("[LetterDropSlot] AnswerChecker.Instance is null");
            }

            // Find AnswerChecker dengan validation yang lebih ketat
            AnswerChecker answerChecker = null;
            
            // Cek Instance dan validate komponen-komponennya
            if (AnswerChecker.Instance != null)
            {
                if (AnswerChecker.Instance.letterSlotParent != null && AnswerChecker.Instance.letterSlotParent.gameObject.activeInHierarchy)
                {
                    answerChecker = AnswerChecker.Instance;
                    Debug.Log("[LetterDropSlot] Using valid AnswerChecker.Instance");
                }
                else
                {
                    Debug.LogWarning("[LetterDropSlot] AnswerChecker.Instance has invalid letterSlotParent, searching for new instance");
                }
            }
            
            // Fallback: cari AnswerChecker yang valid di scene
            if (answerChecker == null)
            {
                AnswerChecker[] answerCheckers = FindObjectsOfType<AnswerChecker>();
                foreach (var checker in answerCheckers)
                {
                    if (checker.letterSlotParent != null && checker.letterSlotParent.gameObject.activeInHierarchy)
                    {
                        answerChecker = checker;
                        Debug.Log("[LetterDropSlot] Found valid AnswerChecker via FindObjectsOfType");
                        break;
                    }
                }
            }

            Debug.Log(
                $"[LetterDropSlot] allFilled={allFilled}, Valid AnswerChecker found? {answerChecker != null}"
            );
            
            if (allFilled && answerChecker != null)
            {
                Debug.Log("[LetterDropSlot] Semua slot terisi, submit jawaban otomatis!");
                try
                {
                    answerChecker.SubmitAnswer();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LetterDropSlot] Error calling SubmitAnswer: {e.Message}");
                }
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
