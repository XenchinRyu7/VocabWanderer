using UnityEngine;
using UnityEngine.UI;

public enum SaveMenuMode { Save, Load }

public class SaveMenuUI : MonoBehaviour
{
    public Transform slotParent; // Assign di inspector ke panel/grid parent
    public GameObject slotPrefab; // Assign di inspector ke prefab row save slot
    public Button saveButton; // Assign di inspector ke button utama (bukan di prefab)
    public SaveMenuMode mode = SaveMenuMode.Save; // Atur di inspector

    private int selectedSlotIndex = -1; // slot yang dipilih user

    void Start()
    {
        RefreshSlots();
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        saveButton.interactable = false;
        saveButton.gameObject.SetActive(mode == SaveMenuMode.Save); // Hide button jika mode load
    }

    public void RefreshSlots()
    {
        foreach (Transform child in slotParent) Destroy(child.gameObject);
        var saves = SaveManager.Instance.saves;
        for (int i = 0; i < saves.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            var ui = slot.GetComponent<SaveSlotUI>();
            ui.SetData(saves[i], false, this, i);
        }
        // Tambah slot kosong untuk new save
        var newSlot = Instantiate(slotPrefab, slotParent);
        var newUi = newSlot.GetComponent<SaveSlotUI>();
        newUi.SetData(null, true, this, saves.Count);
    }

    // Dipanggil dari SaveSlotUI saat slot dipilih
    public void OnSlotSelected(int slotIndex, bool isNew)
    {
        if (mode == SaveMenuMode.Load)
        {
            var saves = SaveManager.Instance.saves;
            if (slotIndex < saves.Count)
            {
                // Simpan index slot yang dipilih ke static agar bisa diakses di scene berikutnya
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
        if (selectedSlotIndex == -1) return;
        var saves = SaveManager.Instance.saves;
        if (selectedSlotIndex < saves.Count)
        {
            // Overwrite
            var data = saves[selectedSlotIndex];
            SaveManager.Instance.OverwriteSave(data.index, "Verb 1, Schema 1", data.schema);
        }
        else
        {
            // New save
            SaveManager.Instance.AddNewSave("Verb 1, Schema 1", "schema_1");
        }
        RefreshSlots();
        saveButton.interactable = false;
        selectedSlotIndex = -1;
    }
}

// Static helper untuk passing index antar scene tanpa PlayerPrefs
public static class SelectedSaveSlot
{
    public static int index = 0;
}
