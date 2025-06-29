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
        // Hanya buat singleton jika BUKAN di scene MainMenu
        // Di MainMenu, biarkan MenuController normal tanpa DontDestroyOnLoad
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainMenu" || currentScene == "LoadGame" || currentScene == "Settings")
        {
            // Di scene menu, tidak perlu singleton
            Debug.Log($"MenuController di scene {currentScene} - tidak menggunakan singleton");
            return;
        }

        // Singleton pattern hanya untuk scene lain
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MenuController initialized as singleton");
        }
        else if (Instance != this)
        {
            Debug.Log("Destroying duplicate MenuController");
            Destroy(gameObject);
            return;
        }
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
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("dialogPanel is null! Please assign it in the Inspector.");
        }
    }

    public void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }

    public void showSaveDialog()
    {
        try
        {
            // Load saveDialogPanel dari prefab jika belum ada atau sudah di-destroy
            if (saveDialogPanel == null || saveDialogPanel.gameObject == null)
            {
                // Try to load from Resources - using SaveMenu prefab
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Save/SaveMenu");
                if (prefab != null)
                {
                    saveDialogPanel = Instantiate(prefab);

                    // Pastikan SaveMenu di-parent ke Canvas yang ada
                    Canvas canvas = FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        saveDialogPanel.transform.SetParent(canvas.transform, false);
                        Debug.Log("SaveMenu di-parent ke Canvas");
                    }

                    // TIDAK pakai DontDestroyOnLoad untuk UI - biar ikut scene
                    Debug.Log("SaveMenu UI berhasil dibuat");
                }
                else
                {
                    Debug.LogError(
                        "SaveMenu prefab tidak ditemukan! Cek Resources/Prefabs/Save/SaveMenu"
                    );
                    return;
                }
            }

            if (saveDialogPanel != null && saveDialogPanel.gameObject != null)
            {
                saveDialogPanel.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error di showSaveDialog: {e.Message}");
        }
    }

    public void hideSaveDialog()
    {
        try
        {
            // Re-find saveDialogPanel jika null atau destroyed
            if (saveDialogPanel == null || saveDialogPanel.gameObject == null)
            {
                // Look for SaveMenu object (instantiated from SaveMenu prefab)
                saveDialogPanel = GameObject.Find("SaveMenu(Clone)");
                if (saveDialogPanel == null)
                {
                    saveDialogPanel = GameObject.Find("SaveMenu");
                }
            }

            if (saveDialogPanel != null && saveDialogPanel.gameObject != null)
            {
                saveDialogPanel.SetActive(false);
            }

            // Re-find dialogPanel jika null atau destroyed
            if (dialogPanel == null || dialogPanel.gameObject == null)
            {
                dialogPanel = GameObject.Find("DialogPanel");
            }

            if (dialogPanel != null && dialogPanel.gameObject != null)
            {
                dialogPanel.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in hideSaveDialog: {e.Message}");
        }
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

        try
        {
            SaveManager saveManager = SaveManager.EnsureInstance();
            if (saveManager == null)
            {
                Debug.LogError("SaveManager tidak bisa dibuat!");
                StartGame(); // Fallback ke game baru
                return;
            }

            var autoSave = saveManager.GetCurrentAutoSave();
            Debug.Log(
                $"AutoSave sebelum continue: schema={autoSave?.schema}, dialogIndex={autoSave?.dialogIndex}, quizIndex={autoSave?.lastCompletedQuizIndex}"
            );

            if (autoSave != null && !string.IsNullOrEmpty(autoSave.schema))
            {
                Debug.Log(
                    $"Melanjutkan game dari autosave: schema={autoSave.schema}, dialogIndex={autoSave.dialogIndex}"
                );

                // Set static variables untuk DialogManager
                DialogManager.lastDialogSceneId = autoSave.schema;
                DialogManager.lastDialogIndex = autoSave.dialogIndex;

                Debug.Log("=== ContinueGame() - Loading DialogScene ===");

                // Langsung load scene tanpa coroutine
                LoadToScene("DialogScene");
            }
            else
            {
                Debug.LogError("Tidak ada data autosave yang valid!");
                StartGame();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error di ContinueGame: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            StartGame(); // Fallback ke game baru
        }
    }

    public bool HasContinueData()
    {
        try
        {
            SaveManager saveManager = SaveManager.EnsureInstance();
            if (saveManager != null)
            {
                var autoSave = saveManager.GetCurrentAutoSave();
                return autoSave != null && !string.IsNullOrEmpty(autoSave.schema);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error checking continue data: {e.Message}");
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

    // Cari MenuController yang ada atau buat baru jika diperlukan
    public static MenuController GetOrCreateInstance()
    {
        // Pertama coba cari yang sudah ada di scene
        MenuController existing = FindObjectOfType<MenuController>();
        if (existing != null)
        {
            Debug.Log("MenuController ditemukan di scene saat ini");
            return existing;
        }

        // Jika tidak ada, cek static Instance
        if (Instance != null && Instance.gameObject != null)
        {
            Debug.Log("MenuController Instance masih valid");
            return Instance;
        }

        // Jika tetap tidak ada, buat baru
        GameObject menuControllerGO = new GameObject("MenuController");
        MenuController newInstance = menuControllerGO.AddComponent<MenuController>();
        Debug.Log("MenuController baru dibuat");

        return newInstance;
    }
}
