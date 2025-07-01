using UnityEngine;
using UnityEngine.UI;

public enum SaveMenuMode
{
    Save,
    Load,
}

public class SaveMenuUI : MonoBehaviour
{
    public Transform slotParent;
    public GameObject slotPrefab;
    public Button saveButton;
    public SaveMenuMode mode = SaveMenuMode.Save;

    private int selectedSlotIndex = -1;

    void Start()
    {
        SetupScrollView(); // Setup scroll view dulu
        RefreshSlots();
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        saveButton.interactable = false;
        saveButton.gameObject.SetActive(mode == SaveMenuMode.Save);
    }

    private void SetupScrollView()
    {
        // Cek apakah sudah ada ScrollView setup
        if (slotParent.GetComponent<ScrollRect>() != null)
        {
            Debug.Log("ScrollView already setup");
            return;
        }

        RectTransform parentRect = slotParent.GetComponent<RectTransform>();
        if (parentRect != null)
        {
            // Set anchor untuk fixed size (top anchor agar mulai dari atas)
            parentRect.anchorMin = new Vector2(0, 1);
            parentRect.anchorMax = new Vector2(1, 1);
            parentRect.sizeDelta = new Vector2(0, 400); // Fixed height 400px
            parentRect.anchoredPosition = new Vector2(0, -294); // Offset dari top untuk space dari "Save Game" text
            Debug.Log("ParentSlot RectTransform updated for fixed height");
        }

        // Setup ScrollView di ParentSlots
        ScrollRect scrollRect = slotParent.gameObject.AddComponent<ScrollRect>();

        // Buat Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(slotParent);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        // Add Mask component
        viewport.AddComponent<Mask>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f); // Almost transparent

        // Buat Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        RectTransform contentRect = content.AddComponent<RectTransform>();

        // PENTING: Content anchor untuk vertical scrolling yang benar
        contentRect.anchorMin = new Vector2(0, 1); // Top-left
        contentRect.anchorMax = new Vector2(1, 1); // Top-right
        contentRect.anchoredPosition = new Vector2(0, 0); // Start dari top
        contentRect.sizeDelta = new Vector2(0, 0); // Will be auto-sized by ContentSizeFitter

        // Setup layout components di Content
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = true; // Biarkan layout control height untuk konsistensi
        vlg.childControlWidth = true; // Control width untuk mengisi penuh
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true; // Force expand width agar slot mengisi lebar penuh

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Setup ScrollRect
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbar = null; // Bisa ditambah scrollbar nanti jika perlu
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        // Update slotParent reference ke Content
        slotParent = content.transform;

        Debug.Log("ScrollView setup completed with proper Content anchoring");
    }

    public void RefreshSlots()
    {
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager == null)
        {
            Debug.LogError("SaveManager not available!");
            return;
        }

        var saves = saveManager.saves;
        for (int i = 0; i < saves.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            var ui = slot.GetComponent<SaveSlotUI>();
            ui.SetData(saves[i], false, this, i);

            // Add swipe handler untuk existing saves
            AddSwipeHandler(slot, i, false);
        }
        var newSlot = Instantiate(slotPrefab, slotParent);
        var newUi = newSlot.GetComponent<SaveSlotUI>();
        newUi.SetData(null, true, this, saves.Count);

        DebugScrollViewHierarchy();

        EnsureSlotLayoutElements();
    }

    private void AddSwipeHandler(GameObject slot, int slotIndex, bool isNewSlot)
    {
        // Add swipe handler component
        SaveSlotSwipeHandler swipeHandler = slot.GetComponent<SaveSlotSwipeHandler>();
        if (swipeHandler == null)
        {
            swipeHandler = slot.AddComponent<SaveSlotSwipeHandler>();
        }

        // Initialize swipe handler
        swipeHandler.Initialize(this, slotIndex, isNewSlot);

        Debug.Log($"Added swipe handler to slot {slotIndex}, isNew: {isNewSlot}");
    }

    private void EnsureSlotLayoutElements()
    {
        foreach (Transform child in slotParent)
        {
            LayoutElement layoutElement = child.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = child.gameObject.AddComponent<LayoutElement>();
            }

            // Set preferred height untuk konsistensi slot
            layoutElement.preferredHeight = 80f; // Tinggi slot yang konsisten
            layoutElement.flexibleWidth = 1f; // Biarkan width flexible mengikuti parent
        }
        Debug.Log($"Layout elements updated for {slotParent.childCount} slots");
    }

    public void OnSlotSelected(int slotIndex, bool isNew)
    {
        if (mode == SaveMenuMode.Load)
        {
            if (!isNew)
            {
                SaveManager saveManager = SaveManager.EnsureInstance();
                if (saveManager != null)
                {
                    saveManager.LoadGameFromSlot(slotIndex + 1);
                    Debug.Log($"Loading game from slot {slotIndex + 1}");
                }
            }
        }
        else // Save mode
        {
            if (isNew)
            {
                // NEW SAVE - langsung save tanpa konfirmasi
                selectedSlotIndex = slotIndex;
                OnSaveButtonClicked(); // Langsung save
                Debug.Log($"New save - directly saving to slot {slotIndex + 1}");
            }
            else
            {
                // EXISTING SAVE - butuh konfirmasi via button
                selectedSlotIndex = slotIndex;
                saveButton.interactable = true;
                saveButton.GetComponentInChildren<Text>().text = "Overwrite";
                Debug.Log(
                    $"Existing save selected - slot {slotIndex + 1}, waiting for overwrite confirmation"
                );
            }
        }
    }

    private void OnSaveButtonClicked()
    {
        if (selectedSlotIndex == -1)
            return;

        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            if (autoSave != null && !string.IsNullOrEmpty(autoSave.schema))
            {
                // Copy dari autosave ke manual save slot
                string saveInfo = $"Save {selectedSlotIndex + 1} - {autoSave.schema}";
                saveManager.CopyAutoSaveToSlot(selectedSlotIndex + 1, saveInfo);
                Debug.Log($"Copied autosave to slot {selectedSlotIndex + 1}");
            }
            else
            {
                Debug.LogError("No autosave data to copy!");
                return;
            }
        }

        RefreshSlots();
        saveButton.interactable = false;
        selectedSlotIndex = -1;
    }

    public void CloseSaveDialog()
    {
        try
        {
            MenuController menuController = FindObjectOfType<MenuController>();

            if (menuController != null)
            {
                Debug.Log("SaveMenuController: Found MenuController, calling hideSaveDialog()");
                menuController.hideSaveDialog();
            }
            else
            {
                Debug.LogError("SaveMenuController: Could not find MenuController in scene!");
                Debug.Log("SaveMenuController: Destroying save dialog as fallback");
                Destroy(gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveMenuController: Error in CloseSaveDialog: {e.Message}");
            Destroy(gameObject);
        }
    }

    public void OnSlotDeleteRequested(int slotIndex)
    {
        ShowDeleteConfirmation(slotIndex);
    }

    private void ShowDeleteConfirmation(int slotIndex)
    {
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager == null || slotIndex >= saveManager.saves.Count)
            return;

        var saveData = saveManager.saves[slotIndex];
        string saveInfo = $"Save {slotIndex + 1}";
        if (!string.IsNullOrEmpty(saveData.schema))
        {
            saveInfo += $" - {saveData.schema}";
        }

        // Show proper confirmation popup
        CreateDeleteConfirmationPopup(slotIndex, saveInfo);
    }

    private void ConfirmDelete(int slotIndex)
    {
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            // Delete save slot
            if (slotIndex < saveManager.saves.Count)
            {
                saveManager.saves.RemoveAt(slotIndex);
                saveManager.SaveAll(); // Save perubahan ke file
                Debug.Log($"Deleted save slot {slotIndex + 1}");

                // Refresh tampilan slot
                RefreshSlots();
            }
            else
            {
                Debug.LogWarning($"Slot index {slotIndex} out of range");
            }
        }
    }

    private void DebugScrollViewHierarchy()
    {
        Debug.Log("=== ScrollView Hierarchy Debug ===");
        Debug.Log($"slotParent: {slotParent.name}");
        Debug.Log($"slotParent.parent: {slotParent.parent.name}");
        Debug.Log($"slotParent.parent.parent: {slotParent.parent.parent.name}");

        RectTransform contentRect = slotParent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            Debug.Log($"Content sizeDelta: {contentRect.sizeDelta}");
            Debug.Log($"Content anchoredPosition: {contentRect.anchoredPosition}");
        }

        ScrollRect scrollRect = slotParent.parent.parent.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            Debug.Log(
                $"ScrollRect found: content={scrollRect.content.name}, viewport={scrollRect.viewport.name}"
            );
        }
        Debug.Log("=== End Debug ===");
    }

    private void CreateDeleteConfirmationPopup(int slotIndex, string saveInfo)
    {
        // Create popup container
        GameObject popup = new GameObject("DeleteConfirmationPopup");
        popup.transform.SetParent(transform.root);

        // Make it cover full screen
        RectTransform popupRect = popup.AddComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.sizeDelta = Vector2.zero;
        popupRect.anchoredPosition = Vector2.zero;

        // Semi-transparent background
        Image popupBg = popup.AddComponent<Image>();
        popupBg.color = new Color(0, 0, 0, 0.7f);

        // Dialog box
        GameObject dialog = new GameObject("Dialog");
        dialog.transform.SetParent(popup.transform);

        RectTransform dialogRect = dialog.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.sizeDelta = new Vector2(300, 150);
        dialogRect.anchoredPosition = Vector2.zero;

        Image dialogBg = dialog.AddComponent<Image>();
        dialogBg.color = Color.white;

        // Title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(dialog.transform);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.6f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = Vector2.zero;

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Delete Save?";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 18;
        titleText.color = Color.black;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;

        // Message text
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(dialog.transform);

        RectTransform msgRect = msgObj.AddComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.1f, 0.3f);
        msgRect.anchorMax = new Vector2(0.9f, 0.6f);
        msgRect.sizeDelta = Vector2.zero;
        msgRect.anchoredPosition = Vector2.zero;

        Text msgText = msgObj.AddComponent<Text>();
        msgText.text = $"Delete {saveInfo}?";
        msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgText.fontSize = 14;
        msgText.color = Color.black;
        msgText.alignment = TextAnchor.MiddleCenter;

        // Buttons container
        GameObject buttonsContainer = new GameObject("ButtonsContainer");
        buttonsContainer.transform.SetParent(dialog.transform);

        RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.1f, 0.05f);
        buttonsRect.anchorMax = new Vector2(0.9f, 0.3f);
        buttonsRect.sizeDelta = Vector2.zero;
        buttonsRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        // Cancel button
        CreatePopupButton(
            buttonsContainer,
            "Cancel",
            Color.gray,
            () =>
            {
                Destroy(popup);
            }
        );

        // Confirm button
        CreatePopupButton(
            buttonsContainer,
            "Delete",
            Color.red,
            () =>
            {
                ConfirmDelete(slotIndex);
                Destroy(popup);
            }
        );
    }

    private void CreatePopupButton(
        GameObject parent,
        string text,
        Color color,
        System.Action onClick
    )
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        Button button = buttonObj.AddComponent<Button>();
        button.image = buttonImage;
        button.onClick.AddListener(() => onClick());

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
    }
}

public static class SelectedSaveSlot
{
    public static int index = 0;
}
