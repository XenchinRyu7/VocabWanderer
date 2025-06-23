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

    void Awake()
    {
        Instance = this;
        savePath = Path.Combine(Application.dataPath, "Resources/Save/saves.json");
        LoadSaves();
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
            schema = schema
        };
        saves.Add(save);
        SaveSlotToFile(save);
        SaveAll();
    }

    public void OverwriteSave(int slotIndex, string info, string schema)
    {
        if (slotIndex < 1 || slotIndex > saves.Count) return;
        var save = saves[slotIndex - 1];
        save.info = info;
        save.dateSaved = DateTime.Now.ToString("dd/MM/yyyy");
        save.schema = schema;
        SaveSlotToFile(save);
        SaveAll();
    }

    private void SaveSlotToFile(SaveData save)
    {
        string dirPath = Path.Combine(Application.dataPath, "Resources/Save");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        string slotPath = Path.Combine(dirPath, $"save{save.index}.json");
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(slotPath, json);
    }
}
