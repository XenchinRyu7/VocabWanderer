using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveSlotSwipeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Swipe Settings")]
    public float swipeThreshold = 100f; // Pixel threshold untuk trigger delete
    public float deleteThreshold = 200f; // Pixel threshold untuk auto-delete
    public float animationSpeed = 5f;

    [Header("UI References")]
    public RectTransform slotContent; // Main slot content yang akan di-slide
    public GameObject deleteOverlay; // Overlay "DELETE" yang muncul di belakang
    public Text deleteText; // Text "DELETE"

    private Vector2 originalPosition;
    private Vector2 startDragPosition;
    private bool isDragging = false;
    private bool deleteRevealed = false;

    // References
    private SaveMenuUI saveMenuUI;
    private int slotIndex;
    private bool isNewSlot;

    void Start()
    {
        // Set original position
        if (slotContent == null)
            slotContent = GetComponent<RectTransform>();

        originalPosition = slotContent.anchoredPosition;

        // Setup delete overlay jika belum ada
        SetupDeleteOverlay();
    }

    private void SetupDeleteOverlay()
    {
        if (deleteOverlay == null)
        {
            // Buat delete overlay di belakang slot content
            GameObject overlay = new GameObject("DeleteOverlay");
            overlay.transform.SetParent(transform);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            overlayRect.anchoredPosition = Vector2.zero;

            // Background merah untuk delete
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(1f, 0.2f, 0.2f, 0.8f); // Red background

            deleteOverlay = overlay;

            // Buat delete text
            GameObject textObj = new GameObject("DeleteText");
            textObj.transform.SetParent(overlay.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            deleteText = textObj.AddComponent<Text>();
            deleteText.text = "DELETE";
            deleteText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            deleteText.fontSize = 18;
            deleteText.color = Color.white;
            deleteText.alignment = TextAnchor.MiddleCenter;
            deleteText.fontStyle = FontStyle.Bold;

            // Hide initially
            deleteOverlay.SetActive(false);
        }

        // Pastikan delete overlay di belakang content
        deleteOverlay.transform.SetSiblingIndex(0);
        slotContent.SetSiblingIndex(1);
    }

    public void Initialize(SaveMenuUI menuUI, int index, bool isNew)
    {
        saveMenuUI = menuUI;
        slotIndex = index;
        isNewSlot = isNew;

        // New slot tidak bisa di-delete
        if (isNewSlot)
        {
            this.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isNewSlot)
            return;

        startDragPosition = eventData.position;
        isDragging = true;

        // Show delete overlay
        deleteOverlay.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isNewSlot || !isDragging)
            return;

        // Calculate drag distance
        Vector2 dragDistance = eventData.position - startDragPosition;

        // Only handle horizontal drag
        float horizontalDrag = dragDistance.x;

        // Clamp drag distance
        horizontalDrag = Mathf.Clamp(horizontalDrag, -deleteThreshold, deleteThreshold);

        // Update slot position
        Vector2 newPosition = originalPosition + new Vector2(horizontalDrag, 0);
        slotContent.anchoredPosition = newPosition;

        // Update delete text based on drag direction and distance
        UpdateDeleteUI(horizontalDrag);
    }

    private void UpdateDeleteUI(float dragDistance)
    {
        float absDrag = Mathf.Abs(dragDistance);

        if (absDrag > swipeThreshold)
        {
            deleteRevealed = true;

            // Change text color based on proximity to delete threshold
            if (absDrag >= deleteThreshold)
            {
                deleteText.text = "RELEASE TO DELETE";
                deleteText.color = Color.yellow;
            }
            else
            {
                deleteText.text = "DELETE";
                deleteText.color = Color.white;
            }
        }
        else
        {
            deleteRevealed = false;
            deleteText.text = "DELETE";
            deleteText.color = Color.white;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isNewSlot || !isDragging)
            return;

        isDragging = false;

        // Calculate final drag distance
        Vector2 dragDistance = eventData.position - startDragPosition;
        float horizontalDrag = dragDistance.x;
        float absDrag = Mathf.Abs(horizontalDrag);

        // Check if delete threshold reached
        if (absDrag >= deleteThreshold && deleteRevealed)
        {
            // Trigger delete
            TriggerDelete();
        }
        else
        {
            // Snap back to original position
            StartCoroutine(AnimateToPosition(originalPosition));
            deleteOverlay.SetActive(false);
        }
    }

    private void TriggerDelete()
    {
        // Animate slide out completely before deleting
        Vector2 slideOutPosition = originalPosition + new Vector2(Screen.width, 0);
        StartCoroutine(AnimateToPositionAndDelete(slideOutPosition));
    }

    private IEnumerator AnimateToPosition(Vector2 targetPosition)
    {
        Vector2 startPos = slotContent.anchoredPosition;
        float journey = 0f;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * animationSpeed;
            slotContent.anchoredPosition = Vector2.Lerp(startPos, targetPosition, journey);
            yield return null;
        }

        slotContent.anchoredPosition = targetPosition;
    }

    private IEnumerator AnimateToPositionAndDelete(Vector2 targetPosition)
    {
        Vector2 startPos = slotContent.anchoredPosition;
        float journey = 0f;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * animationSpeed;
            slotContent.anchoredPosition = Vector2.Lerp(startPos, targetPosition, journey);
            yield return null;
        }

        // Trigger delete after animation
        if (saveMenuUI != null)
        {
            saveMenuUI.OnSlotDeleteRequested(slotIndex);
        }
    }

    // Reset position if needed
    public void ResetPosition()
    {
        slotContent.anchoredPosition = originalPosition;
        deleteOverlay.SetActive(false);
        deleteRevealed = false;
        isDragging = false;
    }
}
