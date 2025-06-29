using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public bool enableSceneChange = true;
    public GameObject dialogPanel;
    public GameObject saveDialogPanel;
    public Button continueButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Script MenuController aktif!");

        if (continueButton != null)
        {
            continueButton.interactable = HasContinueData();
        }
    }

    public void LoadToScene(string sceneName)
    {
        Debug.Log($"Button clicked. Target scene: {sceneName}");

        if (enableSceneChange)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Scene change is disabled. No scene will be loaded.");
        }
    }

    public void ShowDialog()
    {
        dialogPanel.SetActive(true);
    }

    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }

    public void showSaveDialog()
    {
        dialogPanel.SetActive(false);
        saveDialogPanel.SetActive(true);
    }

    public void hideSaveDialog()
    {
        saveDialogPanel.SetActive(false);
        dialogPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit();
    }

    public void StartGame()
    {
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            // Reset dan mulai game baru
            saveManager.AutoSaveProgress("schema_1", 0, 0);
            DialogManager.lastDialogSceneId = "schema_1";
            DialogManager.lastDialogIndex = 0;
            Debug.Log("Started new game with schema_1");
        }
        LoadToScene("DialogScene");
    }

    public void ContinueGame()
    {
        Debug.Log("=== ContinueGame() Begin ===");
        
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            Debug.Log($"AutoSave before continue: schema={autoSave?.schema}, dialogIndex={autoSave?.dialogIndex}, quizIndex={autoSave?.lastCompletedQuizIndex}");
            
            if (autoSave != null && !string.IsNullOrEmpty(autoSave.schema))
            {
                Debug.Log($"Continuing game from autosave: schema={autoSave.schema}, dialogIndex={autoSave.dialogIndex}");
                
                // Set static variables untuk DialogManager
                DialogManager.lastDialogSceneId = autoSave.schema;
                DialogManager.lastDialogIndex = autoSave.dialogIndex;
                
                Debug.Log("=== ContinueGame() - Loading DialogScene ===");
                LoadToScene("DialogScene");
            }
            else
            {
                Debug.LogError("No valid autosave data found!");
                // Fallback ke new game jika tidak ada autosave
                StartGame();
            }
        }
        else
        {
            Debug.LogError("SaveManager could not be created!");
        }
    }

    public bool HasContinueData()
    {
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            return autoSave != null && !string.IsNullOrEmpty(autoSave.schema);
        }
        return false;
    }

    public void ResetAllSaveData()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetAllSaves();
            Debug.Log("All save data has been reset!");
        }
    }

    public void DebugAutoSave()
    {
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            Debug.Log($"Save paths - AutoSave: {Application.persistentDataPath}/autosave.json");
            Debug.Log($"Save paths - Manual: {Application.persistentDataPath}/saves.json");

            if (autoSave != null)
            {
                Debug.Log(
                    $"Auto Save Status: schema={autoSave.schema}, dialogIndex={autoSave.dialogIndex}, quizIndex={autoSave.lastCompletedQuizIndex}, lastPlayTime={autoSave.lastPlayTime}"
                );
            }
            else
            {
                Debug.Log("No auto save data found!");
            }
        }
    }
}
