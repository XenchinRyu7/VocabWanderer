using System.IO;
using UnityEngine;

public class SaveSystemTest : MonoBehaviour
{
    [Header("Debug Save System")]
    [SerializeField]
    private bool testOnStart = false;

    [SerializeField]
    private string testSchema = "TestSchema";

    [SerializeField]
    private int testDialogIndex = 5;

    [SerializeField]
    private int testQuizIndex = 3;

    void Start()
    {
        if (testOnStart)
        {
            TestSaveSystem();
        }
    }

    [ContextMenu("Test Save System")]
    public void TestSaveSystem()
    {
        Debug.Log("=== TESTING SAVE SYSTEM ===");

        // Test SaveManager creation
        Debug.Log("1. Testing SaveManager.EnsureInstance()...");
        SaveManager saveManager = SaveManager.EnsureInstance();

        if (saveManager == null)
        {
            Debug.LogError("FAILED: SaveManager.EnsureInstance() returned null!");
            return;
        }

        Debug.Log($"SUCCESS: SaveManager created. Instance: {saveManager != null}");
        Debug.Log($"SaveManager GameObject: {saveManager.gameObject.name}");
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");

        // Test auto save
        Debug.Log("2. Testing auto save...");
        try
        {
            saveManager.UpdateProgress(testSchema, testDialogIndex, testQuizIndex);
            Debug.Log("SUCCESS: Auto save completed without errors");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FAILED: Auto save threw exception: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            return;
        }

        // Test file creation
        Debug.Log("3. Testing file creation...");
        string autoSavePath = Path.Combine(Application.persistentDataPath, "autosave.json");

        if (File.Exists(autoSavePath))
        {
            Debug.Log($"SUCCESS: Auto save file created at: {autoSavePath}");

            // Read and display the content
            try
            {
                string content = File.ReadAllText(autoSavePath);
                Debug.Log($"Auto save content: {content}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FAILED: Could not read auto save file: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"FAILED: Auto save file not found at: {autoSavePath}");
        }

        // Test loading auto save
        Debug.Log("4. Testing auto save loading...");
        try
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            if (autoSave != null)
            {
                Debug.Log(
                    $"SUCCESS: Auto save loaded - Schema: {autoSave.schema}, Dialog: {autoSave.dialogIndex}, Quiz: {autoSave.lastCompletedQuizIndex}"
                );
            }
            else
            {
                Debug.LogError("FAILED: GetCurrentAutoSave() returned null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FAILED: Loading auto save threw exception: {e.Message}");
        }

        Debug.Log("=== SAVE SYSTEM TEST COMPLETE ===");
    }

    [ContextMenu("Reset All Saves")]
    public void TestResetAllSaves()
    {
        Debug.Log("=== TESTING RESET ALL SAVES ===");

        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            saveManager.ResetAllSaves();
            Debug.Log("Reset all saves completed");
        }
        else
        {
            Debug.LogError("Cannot reset saves - SaveManager is null");
        }
    }

    [ContextMenu("Show Save File Paths")]
    public void ShowSaveFilePaths()
    {
        Debug.Log("=== SAVE FILE PATHS ===");
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
        Debug.Log(
            $"Auto Save Path: {Path.Combine(Application.persistentDataPath, "autosave.json")}"
        );
        Debug.Log(
            $"Manual Saves Path: {Path.Combine(Application.persistentDataPath, "saves.json")}"
        );

        // Check which files exist
        string autoSavePath = Path.Combine(Application.persistentDataPath, "autosave.json");
        string saveListPath = Path.Combine(Application.persistentDataPath, "saves.json");

        Debug.Log($"Auto save exists: {File.Exists(autoSavePath)}");
        Debug.Log($"Save list exists: {File.Exists(saveListPath)}");

        // List all save files
        string saveDir = Application.persistentDataPath;
        if (Directory.Exists(saveDir))
        {
            string[] saveFiles = Directory.GetFiles(saveDir, "*.json");
            Debug.Log($"Found {saveFiles.Length} save files:");
            foreach (string file in saveFiles)
            {
                Debug.Log($"  - {Path.GetFileName(file)}");
            }
        }
    }
}
