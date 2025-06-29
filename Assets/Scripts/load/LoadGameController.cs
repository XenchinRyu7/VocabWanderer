using UnityEngine;

public class LoadGameController : MonoBehaviour
{
    public void LoadGameBySchema(string schema)
    {
        if (SaveManager.Instance != null)
        {
            SaveData save = SaveManager.Instance.GetSaveBySchema(schema);
            if (save != null)
            {
                SaveManager.Instance.LoadGameState(save);
            }
            else
            {
                Debug.LogError($"Save data not found for schema: {schema}");
            }
        }
    }

    // Contoh method untuk load game dari UI button
    public void LoadSchema1()
    {
        LoadGameBySchema("schema_1");
    }

    public void LoadSchema2()
    {
        LoadGameBySchema("schema_2");
    }
}
