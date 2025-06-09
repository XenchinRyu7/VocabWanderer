using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class LetterDropSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public TextMeshProUGUI displayText;
    private char? currentLetter = null;

    public Sprite filledSprite; // assign via inspector sprite filled (box terisi)
    private UnityEngine.UI.Image slotImage; // tidak perlu assign manual

    void Awake() {
        slotImage = GetComponent<UnityEngine.UI.Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetter dragged = eventData.pointerDrag.GetComponent<DraggableLetter>();
        if (dragged != null && !currentLetter.HasValue)
        {
            // Ambil huruf dari tile yang didrag
            currentLetter = dragged.letter;
            displayText.text = currentLetter.ToString();
            Destroy(dragged.gameObject); // Hapus tile huruf setelah dipakai
            // Ganti sprite background box ke filled
            if (filledSprite != null && slotImage != null)
                slotImage.sprite = filledSprite;
            // Setelah terisi, slot tidak bisa diisi lagi
            this.enabled = false;
            // --- Tambahan: Auto-submit jika semua slot sudah terisi ---
            var allSlots = transform.parent.GetComponentsInChildren<LetterDropSlot>();
            bool allFilled = true;
            foreach (var slot in allSlots) {
                if (!slot.GetCurrentLetter().HasValue) { allFilled = false; break; }
            }
            Debug.Log($"[LetterDropSlot] allFilled={allFilled}, AnswerChecker.Instance null? {AnswerChecker.Instance == null}");
            if (allFilled && AnswerChecker.Instance != null) {
                Debug.Log("[LetterDropSlot] Semua slot terisi, submit jawaban otomatis!");
                AnswerChecker.Instance.SubmitAnswer();
            }
            // --- End tambahan ---
        }
    }

    public char? GetCurrentLetter() => currentLetter;
    public void ClearSlot() { currentLetter = null; displayText.text = ""; }
}
