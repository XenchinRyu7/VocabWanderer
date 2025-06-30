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
        Debug.Log(
            $"MenuController di scene {SceneManager.GetActiveScene().name} - normal instance"
        );
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
        try
        {
            Debug.Log("=== ShowDialog() started ===");

            // Close existing dialog first
            if (dialogPanel != null)
            {
                Debug.Log("Destroying existing dialogPanel");
                DestroyImmediate(dialogPanel);
                dialogPanel = null;
            }

            // Find the scene Canvas
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            Canvas sceneCanvas = null;

            foreach (Canvas canvas in canvases)
            {
                // Skip Canvas in DontDestroyOnLoad
                if (canvas.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    sceneCanvas = canvas;
                    break;
                }
            }

            if (sceneCanvas == null)
            {
                Debug.LogError("No Canvas found in current scene!");
                return;
            }

            // Load DialogPanel prefab
            GameObject dialogPrefab = Resources.Load<GameObject>("Prefabs/Dialog/DialogPanel");
            if (dialogPrefab != null)
            {
                Debug.Log("DialogPanel prefab loaded successfully");

                // Instantiate the prefab
                GameObject dialogInstance = Instantiate(dialogPrefab, sceneCanvas.transform);
                Debug.Log($"DialogPanel instantiated: {dialogInstance.name}");

                // Setup proper parenting and positioning
                RectTransform dialogRect = dialogInstance.GetComponent<RectTransform>();
                if (dialogRect != null)
                {
                    dialogRect.anchorMin = Vector2.zero;
                    dialogRect.anchorMax = Vector2.one;
                    dialogRect.sizeDelta = Vector2.zero;
                    dialogRect.anchoredPosition = Vector2.zero;
                }

                // Force to front
                dialogInstance.transform.SetAsLastSibling();

                dialogPanel = dialogInstance;

                Debug.Log($"DialogPanel from prefab successfully displayed: {dialogInstance.name}");
                Debug.Log($"DialogPanel active: {dialogInstance.activeSelf}");
                Debug.Log($"DialogPanel parent: {dialogInstance.transform.parent.name}");
            }
            else
            {
                Debug.LogError(
                    "DialogPanel prefab could not be loaded from Resources/Prefabs/Dialog/DialogPanel!"
                );
            }

            Debug.Log("=== ShowDialog() completed ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ShowDialog: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public void HideDialog()
    {
        try
        {
            if (dialogPanel != null)
            {
                Debug.Log("Hiding Dialog panel");
                DestroyImmediate(dialogPanel);
                dialogPanel = null;
            }
            else
            {
                Debug.Log("dialogPanel is null, nothing to hide");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HideDialog: {e.Message}");
        }
    }

    public void showSaveDialog()
    {
        try
        {
            Debug.Log("=== showSaveDialog() started ===");

            if (saveDialogPanel != null)
            {
                Debug.Log("Destroying existing saveDialogPanel");
                DestroyImmediate(saveDialogPanel);
                saveDialogPanel = null;
            }

            Canvas[] canvases = FindObjectsOfType<Canvas>();
            Canvas sceneCanvas = null;

            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    sceneCanvas = canvas;
                    break;
                }
            }

            if (sceneCanvas == null)
            {
                Debug.LogError("No Canvas found in current scene!");
                return;
            }

            GameObject saveMenuPrefab = Resources.Load<GameObject>("Prefabs/Save/SaveMenu");
            if (saveMenuPrefab != null)
            {
                Debug.Log("SaveMenu prefab loaded successfully");

                GameObject saveMenuInstance = Instantiate(saveMenuPrefab, sceneCanvas.transform);
                Debug.Log($"SaveMenu instantiated: {saveMenuInstance.name}");

                RectTransform saveRect = saveMenuInstance.GetComponent<RectTransform>();
                if (saveRect != null)
                {
                    saveRect.anchorMin = Vector2.zero;
                    saveRect.anchorMax = Vector2.one;
                    saveRect.sizeDelta = Vector2.zero;
                    saveRect.anchoredPosition = Vector2.zero;
                }

                saveMenuInstance.transform.SetAsLastSibling();

                // Add SaveMenuController script if it doesn't exist
                SaveMenuController saveMenuController =
                    saveMenuInstance.GetComponent<SaveMenuController>();
                if (saveMenuController == null)
                {
                    saveMenuController = saveMenuInstance.AddComponent<SaveMenuController>();
                    Debug.Log("Added SaveMenuController script to SaveMenu instance");
                }

                saveDialogPanel = saveMenuInstance;

                Debug.Log($"SaveMenu from prefab successfully displayed: {saveMenuInstance.name}");
                Debug.Log($"SaveMenu active: {saveMenuInstance.activeSelf}");
                Debug.Log($"SaveMenu parent: {saveMenuInstance.transform.parent.name}");
            }
            else
            {
                Debug.LogError(
                    "SaveMenu prefab could not be loaded from Resources/Prefabs/Save/SaveMenu!"
                );
            }

            Debug.Log("=== showSaveDialog() completed ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in showSaveDialog: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public void hideSaveDialog()
    {
        try
        {
            if (saveDialogPanel != null)
            {
                Debug.Log("Hiding SaveMenu dialog");
                DestroyImmediate(saveDialogPanel);
                saveDialogPanel = null;
            }
            else
            {
                Debug.Log("saveDialogPanel is null, nothing to hide");
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
                StartGame();
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

                DialogManager.lastDialogSceneId = autoSave.schema;
                DialogManager.lastDialogIndex = autoSave.dialogIndex;

                Debug.Log("=== ContinueGame() - Loading DialogScene ===");

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
            StartGame();
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

    public static MenuController GetOrCreateInstance()
    {
        MenuController existing = FindObjectOfType<MenuController>();
        if (existing != null)
        {
            Debug.Log("MenuController ditemukan di scene saat ini");
            return existing;
        }

        GameObject menuControllerGO = new GameObject("MenuController");
        MenuController newInstance = menuControllerGO.AddComponent<MenuController>();
        Debug.Log("MenuController baru dibuat di scene saat ini");

        return newInstance;
    }
}
