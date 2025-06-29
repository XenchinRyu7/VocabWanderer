using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int index;
    public string info;
    public string dateSaved;
    public string schema;
    public int dialogIndex = 0;
    public int lastCompletedQuizIndex = 0;
}

[Serializable]
public class AutoSaveData
{
    public string schema;
    public int dialogIndex = 0;
    public int lastCompletedQuizIndex = 0;
    public string lastPlayTime;
}

[Serializable]
public class SaveListWrapper
{
    public List<SaveData> list = new List<SaveData>();
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public List<SaveData> saves = new List<SaveData>();
    private string savePath;
    private string autoSavePath;
    private AutoSaveData currentAutoSave;

    // Ensure SaveManager exists
    public static SaveManager EnsureInstance()
    {
        // Check if Instance exists and is not destroyed
        if (Instance == null || (Instance != null && Instance.gameObject == null))
        {
            // Clear the reference if it's pointing to a destroyed object
            Instance = null;

            GameObject saveManagerGO = new GameObject("SaveManager");
            Instance = saveManagerGO.AddComponent<SaveManager>();
            DontDestroyOnLoad(saveManagerGO);

            // Initialize paths manually since Awake might not be called immediately
            Instance.savePath = Path.Combine(Application.persistentDataPath, "saves.json");
            Instance.autoSavePath = Path.Combine(Application.persistentDataPath, "autosave.json");
            Instance.LoadSaves();
            Instance.LoadAutoSave();

            Debug.Log($"SaveManager created dynamically at path: {Application.persistentDataPath}");
        }
        else if (Instance != null)
        {
            Debug.Log("SaveManager instance already exists and is valid");
        }

        return Instance;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "saves.json");
            autoSavePath = Path.Combine(Application.persistentDataPath, "autosave.json");
            LoadSaves();
            LoadAutoSave();
            Debug.Log("SaveManager Awake - Instance set and initialized");
        }
        else if (Instance != this)
        {
            Debug.Log("SaveManager Awake - Destroying duplicate instance");
            Destroy(gameObject);
        }
    }

    public void LoadSaves()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            saves = JsonUtility.FromJson<SaveListWrapper>(json).list;
        }
        else
        {
            saves = new List<SaveData>();
        }
    }

    public void SaveAll()
    {
        var wrapper = new SaveListWrapper { list = saves };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, json);
    }

    public void AddNewSave(string info, string schema)
    {
        int newIndex = saves.Count + 1;
        var save = new SaveData
        {
            index = newIndex,
            info = info,
            dateSaved = DateTime.Now.ToString("dd/MM/yyyy"),
            schema = schema,
        };
        saves.Add(save);
        SaveSlotToFile(save);
        SaveAll();
    }

    public void OverwriteSave(int slotIndex, string info, string schema)
    {
        if (slotIndex < 1 || slotIndex > saves.Count)
            return;
        var save = saves[slotIndex - 1];
        save.info = info;
        save.dateSaved = DateTime.Now.ToString("dd/MM/yyyy");
        save.schema = schema;
        SaveSlotToFile(save);
        SaveAll();
    }

    public void UpdateProgress(string schema, int dialogIndex, int lastCompletedQuizIndex)
    {
        if (string.IsNullOrEmpty(autoSavePath))
        {
            autoSavePath = Path.Combine(Application.persistentDataPath, "autosave.json");
            Debug.LogWarning("AutoSavePath was not initialized, setting it now.");
        }

        AutoSaveProgress(schema, dialogIndex, lastCompletedQuizIndex);
    }

    public SaveData GetSaveBySchema(string schema)
    {
        return saves.Find(s => s.schema == schema);
    }

    public void LoadGameState(SaveData save)
    {
        if (save != null)
        {
            DialogManager.lastDialogSceneId = save.schema;
            DialogManager.lastDialogIndex = save.dialogIndex;

            Debug.Log(
                $"Loading game state: schema={save.schema}, dialogIndex={save.dialogIndex}, lastCompletedQuizIndex={save.lastCompletedQuizIndex}"
            );

            UnityEngine.SceneManagement.SceneManager.LoadScene("DialogScene");
        }
    }

    private void SaveSlotToFile(SaveData save)
    {
        string saveDir = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);
        string slotPath = Path.Combine(saveDir, $"save{save.index}.json");
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(slotPath, json);
        Debug.Log($"Saved slot {save.index} to: {slotPath}");
    }

    public void LoadAutoSave()
    {
        if (File.Exists(autoSavePath))
        {
            string json = File.ReadAllText(autoSavePath);
            currentAutoSave = JsonUtility.FromJson<AutoSaveData>(json);
        }
        else
        {
            currentAutoSave = new AutoSaveData();
        }
    }

    public void AutoSaveProgress(string schema, int dialogIndex, int lastCompletedQuizIndex)
    {
        if (currentAutoSave == null)
        {
            currentAutoSave = new AutoSaveData();
            Debug.LogWarning("currentAutoSave was null, creating new instance.");
        }

        if (string.IsNullOrEmpty(autoSavePath))
        {
            autoSavePath = Path.Combine(Application.persistentDataPath, "autosave.json");
            Debug.LogWarning("autoSavePath was not set, initializing it now.");
        }

        currentAutoSave.schema = schema;
        currentAutoSave.dialogIndex = dialogIndex;
        currentAutoSave.lastCompletedQuizIndex = lastCompletedQuizIndex;
        currentAutoSave.lastPlayTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        string saveDir = Path.GetDirectoryName(autoSavePath);
        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);

        string json = JsonUtility.ToJson(currentAutoSave, true);
        File.WriteAllText(autoSavePath, json);

        Debug.Log($"Auto saved to: {autoSavePath}");
        Debug.Log(
            $"Auto saved: schema={schema}, dialogIndex={dialogIndex}, lastCompletedQuizIndex={lastCompletedQuizIndex}"
        );
    }

    public AutoSaveData GetCurrentAutoSave()
    {
        return currentAutoSave;
    }

    public void CopyAutoSaveToSlot(int slotIndex, string saveInfo)
    {
        if (currentAutoSave != null && !string.IsNullOrEmpty(currentAutoSave.schema))
        {
            if (slotIndex <= saves.Count)
            {
                OverwriteSaveFromAutoSave(slotIndex, saveInfo);
            }
            else
            {
                AddNewSaveFromAutoSave(saveInfo);
            }
            Debug.Log($"Auto save copied to slot {slotIndex}");
        }
    }

    private void AddNewSaveFromAutoSave(string info)
    {
        int newIndex = saves.Count + 1;
        var save = new SaveData
        {
            index = newIndex,
            info = info,
            dateSaved = DateTime.Now.ToString("dd/MM/yyyy"),
            schema = currentAutoSave.schema,
            dialogIndex = currentAutoSave.dialogIndex,
            lastCompletedQuizIndex = currentAutoSave.lastCompletedQuizIndex,
        };
        saves.Add(save);
        SaveSlotToFile(save);
        SaveAll();
    }

    private void OverwriteSaveFromAutoSave(int slotIndex, string info)
    {
        if (slotIndex < 1 || slotIndex > saves.Count)
            return;
        var save = saves[slotIndex - 1];
        save.info = info;
        save.dateSaved = DateTime.Now.ToString("dd/MM/yyyy");
        save.schema = currentAutoSave.schema;
        save.dialogIndex = currentAutoSave.dialogIndex;
        save.lastCompletedQuizIndex = currentAutoSave.lastCompletedQuizIndex;
        SaveSlotToFile(save);
        SaveAll();
    }

    public void LoadGameFromAutoSave()
    {
        if (currentAutoSave != null && !string.IsNullOrEmpty(currentAutoSave.schema))
        {
            DialogManager.lastDialogSceneId = currentAutoSave.schema;
            DialogManager.lastDialogIndex = currentAutoSave.dialogIndex;

            Debug.Log(
                $"Loading from auto save: schema={currentAutoSave.schema}, dialogIndex={currentAutoSave.dialogIndex}"
            );

            UnityEngine.SceneManagement.SceneManager.LoadScene("DialogScene");
        }
        else
        {
            Debug.LogError("No auto save data available!");
        }
    }

    public void LoadGameFromSlot(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > saves.Count)
            return;

        var save = saves[slotIndex - 1];
        currentAutoSave.schema = save.schema;
        currentAutoSave.dialogIndex = save.dialogIndex;
        currentAutoSave.lastCompletedQuizIndex = save.lastCompletedQuizIndex;

        LoadGameState(save);
    }

    public void ResetAllSaves()
    {
        // Reset auto save
        currentAutoSave = new AutoSaveData();
        if (File.Exists(autoSavePath))
            File.Delete(autoSavePath);

        // Reset manual saves
        saves.Clear();
        if (File.Exists(savePath))
            File.Delete(savePath);

        // Delete individual save files
        string saveDir = Path.GetDirectoryName(savePath);
        if (Directory.Exists(saveDir))
        {
            string[] saveFiles = Directory.GetFiles(saveDir, "save*.json");
            foreach (string file in saveFiles)
            {
                File.Delete(file);
            }
        }

        Debug.Log("All save data reset successfully!");
    }
}
