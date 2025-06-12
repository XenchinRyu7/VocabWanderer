using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public bool enableSceneChange = true;
    public GameObject dialogPanel;
    public GameObject saveDialogPanel;

    void Awake() {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Script MenuController aktif!");
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
        // Jika belum ada save, buat save baru dengan schema default
        if (SaveManager.Instance != null && (SaveManager.Instance.saves == null || SaveManager.Instance.saves.Count == 0))
        {
            SaveManager.Instance.AddNewSave("Verb 1, Schema 1", "schema_1");
        }
        // Pindah ke DialogScene
        LoadToScene("DialogScene");
    }
}
