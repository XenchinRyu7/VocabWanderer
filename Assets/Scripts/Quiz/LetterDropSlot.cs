using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class LetterDropSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public TextMeshProUGUI displayText;
    private char? currentLetter = null;

    public Sprite filledSprite; // assign via inspector jika ingin efek visual
    public UnityEngine.UI.Image slotImage; // assign via inspector jika ingin efek visual

    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetter dragged = eventData.pointerDrag.GetComponent<DraggableLetter>();
        if (dragged != null && !currentLetter.HasValue)
        {
            // Ambil huruf dari tile yang didrag
            currentLetter = dragged.letter;
            displayText.text = currentLetter.ToString();
            Destroy(dragged.gameObject); // Hapus tile huruf setelah dipakai
            // Jika ingin slot berubah tampilan saat terisi:
            if (filledSprite != null && slotImage != null)
                slotImage.sprite = filledSprite;
            // Setelah terisi, slot tidak bisa diisi lagi
            this.enabled = false;
        }
    }

    public char? GetCurrentLetter() => currentLetter;
    public void ClearSlot() { currentLetter = null; displayText.text = ""; }
}
