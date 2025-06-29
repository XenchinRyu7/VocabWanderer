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
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AutoSaveProgress("schema_1", 0, 0);
            DialogManager.lastDialogSceneId = "schema_1";
            DialogManager.lastDialogIndex = 0;
        }
        LoadToScene("DialogScene");
    }

    public void ContinueGame()
    {
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            saveManager.LoadGameFromAutoSave();
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
