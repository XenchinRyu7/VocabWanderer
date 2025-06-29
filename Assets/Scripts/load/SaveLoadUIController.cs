using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUIController : MonoBehaviour
{
    [Header("UI References")]
    public Button continueButton; // Continue dari auto save
    public Button[] loadSlotButtons; // Load dari slot tertentu
    public Button[] saveSlotButtons; // Save ke slot tertentu
    public InputField saveInfoInput; // Input untuk info save
    
    void Start()
    {
        SetupButtons();
        UpdateContinueButton();
    }
    
    void SetupButtons()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueGame);
            
        for (int i = 0; i < loadSlotButtons.Length; i++)
        {
            int slotIndex = i + 1; // Slot dimulai dari 1
            if (loadSlotButtons[i] != null)
                loadSlotButtons[i].onClick.AddListener(() => OnLoadSlot(slotIndex));
        }
        
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotIndex = i + 1; // Slot dimulai dari 1
            if (saveSlotButtons[i] != null)
                saveSlotButtons[i].onClick.AddListener(() => OnSaveToSlot(slotIndex));
        }
    }
    
    void UpdateContinueButton()
    {
        if (continueButton != null && SaveManager.Instance != null)
        {
            var autoSave = SaveManager.Instance.GetCurrentAutoSave();
            bool hasAutoSave = autoSave != null && !string.IsNullOrEmpty(autoSave.schema);
            continueButton.interactable = hasAutoSave;
            
            if (hasAutoSave)
            {
                continueButton.GetComponentInChildren<Text>().text = $"Continue ({autoSave.schema})";
            }
            else
            {
                continueButton.GetComponentInChildren<Text>().text = "Continue (No Data)";
            }
        }
    }
    
    public void OnContinueGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGameFromAutoSave();
        }
    }
    
    public void OnLoadSlot(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGameFromSlot(slotIndex);
        }
    }
    
    public void OnSaveToSlot(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            string saveInfo = "Game Progress";
            if (saveInfoInput != null && !string.IsNullOrEmpty(saveInfoInput.text))
            {
                saveInfo = saveInfoInput.text;
            }
            
            SaveManager.Instance.CopyAutoSaveToSlot(slotIndex, saveInfo);
            
            // Update UI atau beri feedback ke user
            Debug.Log($"Game saved to slot {slotIndex}");
        }
    }
    
    public void OnNewGame()
    {
        // Start new game dari schema_1
        if (SaveManager.Instance != null)
        {
            // Reset auto save
            SaveManager.Instance.AutoSaveProgress("schema_1", 0, 0);
            SaveManager.Instance.LoadGameFromAutoSave();
        }
    }
}
