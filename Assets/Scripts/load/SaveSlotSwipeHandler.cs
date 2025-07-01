using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveSlotSwipeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Swipe Settings")]
    public float swipeThreshold = 100f;
    public float deleteThreshold = 200f;
    public float animationSpeed = 5f;

    [Header("UI References")]
    public RectTransform slotContent;
    public GameObject deleteOverlay;
    public Text deleteText;

    private Vector2 originalPosition;
    private Vector2 startDragPosition;
    private bool isDragging = false;
    private bool deleteRevealed = false;

    private SaveMenuUI saveMenuUI;
    private int slotIndex;
    private bool isNewSlot;

    void Start()
    {
        if (slotContent == null)
            slotContent = GetComponent<RectTransform>();

        originalPosition = slotContent.anchoredPosition;

        SetupDeleteOverlay();
    }

    private void SetupDeleteOverlay()
    {
        if (deleteOverlay == null)
        {
            GameObject overlay = new GameObject("DeleteOverlay");
            overlay.transform.SetParent(transform);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            overlayRect.anchoredPosition = Vector2.zero;

            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(1f, 0.2f, 0.2f, 0.8f);

            deleteOverlay = overlay;

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

            deleteOverlay.SetActive(false);
        }

        deleteOverlay.transform.SetSiblingIndex(0);
        slotContent.SetSiblingIndex(1);
    }

    public void Initialize(SaveMenuUI menuUI, int index, bool isNew)
    {
        saveMenuUI = menuUI;
        slotIndex = index;
        isNewSlot = isNew;

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

        deleteOverlay.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isNewSlot || !isDragging)
            return;

        Vector2 dragDistance = eventData.position - startDragPosition;

        float horizontalDrag = dragDistance.x;

        horizontalDrag = Mathf.Clamp(horizontalDrag, -deleteThreshold, deleteThreshold);

        Vector2 newPosition = originalPosition + new Vector2(horizontalDrag, 0);
        slotContent.anchoredPosition = newPosition;

        UpdateDeleteUI(horizontalDrag);
    }

    private void UpdateDeleteUI(float dragDistance)
    {
        float absDrag = Mathf.Abs(dragDistance);

        if (absDrag > swipeThreshold)
        {
            deleteRevealed = true;

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

        Vector2 dragDistance = eventData.position - startDragPosition;
        float horizontalDrag = dragDistance.x;
        float absDrag = Mathf.Abs(horizontalDrag);

        if (absDrag >= deleteThreshold && deleteRevealed)
        {
            TriggerDelete();
        }
        else
        {
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

        if (saveMenuUI != null)
        {
            saveMenuUI.OnSlotDeleteRequested(slotIndex);
        }
    }

    public void ResetPosition()
    {
        slotContent.anchoredPosition = originalPosition;
        deleteOverlay.SetActive(false);
        deleteRevealed = false;
        isDragging = false;
    }
}
