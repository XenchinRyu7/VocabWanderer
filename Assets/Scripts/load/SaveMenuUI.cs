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
        RefreshSlots();
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        saveButton.interactable = false;
        saveButton.gameObject.SetActive(mode == SaveMenuMode.Save);
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
        }
        var newSlot = Instantiate(slotPrefab, slotParent);
        var newUi = newSlot.GetComponent<SaveSlotUI>();
        newUi.SetData(null, true, this, saves.Count);
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
}

public static class SelectedSaveSlot
{
    public static int index = 0;
}
