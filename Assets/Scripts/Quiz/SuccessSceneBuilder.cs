using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SuccessSceneBuilder : MonoBehaviour
{
    public Transform starParent;
    public GameObject starOnPrefab;
    public Text scoreText;
    public Button continueDialogButton;

    void Start()
    {
        int health = GameManager.lastHealth;
        float totalTime = GameManager.lastTotalTime;

        int score = (health * 100) + Mathf.Max(0, 500 - Mathf.RoundToInt(totalTime) * 10);
        int stars = 1;
        if (score >= 400)
            stars = 3;
        else if (score >= 250)
            stars = 2;

        if (scoreText != null)
            scoreText.text = $"Scene.. Complete\nHeart : {health}\nScore : {score}";

        for (int i = 0; i < stars; i++)
        {
            Instantiate(starOnPrefab, starParent);
        }

        if (continueDialogButton != null)
            continueDialogButton.onClick.AddListener(OnContinueDialogClicked);
    }

    public void OnContinueDialogClicked()
    {
        Debug.Log("SuccessSceneBuilder: OnContinueDialogClicked");

        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            if (autoSave != null && !string.IsNullOrEmpty(autoSave.schema))
            {
                Debug.Log(
                    $"[SuccessSceneBuilder] Current AutoSave after quiz completion: schema={autoSave.schema}, dialogIndex={autoSave.dialogIndex}, quizIndex={autoSave.lastCompletedQuizIndex}"
                );

                DialogManager.lastDialogSceneId = autoSave.schema;
                DialogManager.lastDialogIndex = autoSave.dialogIndex;

                Debug.Log(
                    $"[SuccessSceneBuilder] Continuing dialog after quiz: schema={autoSave.schema}, dialogIndex={autoSave.dialogIndex}"
                );
            }
            else
            {
                Debug.LogError("[SuccessSceneBuilder] No valid autosave found!");
            }
        }

        // Load DialogScene directly
        SceneManager.LoadScene("DialogScene");
    }

    public void OnHomeClicked()
    {
        Debug.Log("SuccessSceneBuilder: Home button clicked");

        try
        {
            // Load scene MainMenu langsung
            SceneManager.LoadScene("MainMenu");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SuccessSceneBuilder: Error in OnHomeClicked: {e.Message}");
        }
    }

    public void OnSaveClicked()
    {
        Debug.Log("SuccessSceneBuilder: Save button clicked");

        try
        {
            // Find MenuController in current scene
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController != null)
            {
                menuController.showSaveDialog();
            }
            else
            {
                Debug.LogWarning(
                    "SuccessSceneBuilder: MenuController not found in current scene for save dialog!"
                );
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SuccessSceneBuilder: Error in OnSaveClicked: {e.Message}");
        }
    }

    public void ShowDialog()
    {
        Debug.Log("SuccessSceneBuilder: ShowDialog called");

        try
        {
            // Find MenuController in current scene
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController != null)
            {
                menuController.ShowDialog();
            }
            else
            {
                Debug.LogWarning(
                    "SuccessSceneBuilder: MenuController not found in current scene for dialog!"
                );
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SuccessSceneBuilder: Error in ShowDialog: {e.Message}");
        }
    }
}
