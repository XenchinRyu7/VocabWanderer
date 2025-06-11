using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public bool enableSceneChange = true;
    public GameObject dialogPanel;

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
    

    public void ExitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit();
    }
}
