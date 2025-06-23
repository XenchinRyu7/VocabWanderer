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
        var saves = SaveManager.Instance.saves;
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
            var saves = SaveManager.Instance.saves;
            if (slotIndex < saves.Count)
            {
                SelectedSaveSlot.index = slotIndex;
                UnityEngine.SceneManagement.SceneManager.LoadScene("DialogScene");
            }
        }
        else
        {
            selectedSlotIndex = slotIndex;
            saveButton.interactable = true;
            saveButton.GetComponentInChildren<Text>().text = isNew ? "Save" : "Overwrite";
        }
    }

    private void OnSaveButtonClicked()
    {
        if (selectedSlotIndex == -1)
            return;
        var saves = SaveManager.Instance.saves;
        if (selectedSlotIndex < saves.Count)
        {
            var data = saves[selectedSlotIndex];
            SaveManager.Instance.OverwriteSave(data.index, "Verb 1, Schema 1", data.schema);
        }
        else
        {
            SaveManager.Instance.AddNewSave("Verb 1, Schema 1", "schema_1");
        }
        RefreshSlots();
        saveButton.interactable = false;
        selectedSlotIndex = -1;
    }
}

public static class SelectedSaveSlot
{
    public static int index = 0;
}
