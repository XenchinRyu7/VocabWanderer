using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public char letter;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Vector2 originalSizeDelta;
    private Canvas canvas;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalSizeDelta = rectTransform.sizeDelta;
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null || !eventData.pointerEnter.GetComponent<LetterDropSlot>())
        {
            transform.SetParent(originalParent, true);
            transform.position = originalPosition;
            rectTransform.sizeDelta = originalSizeDelta;
        }
        canvasGroup.blocksRaycasts = true;
    }
}
