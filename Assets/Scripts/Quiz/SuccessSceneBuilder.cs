using UnityEngine;
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
        // Pastikan dialog melanjutkan dari autosave setelah quiz
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            if (autoSave != null && !string.IsNullOrEmpty(autoSave.schema))
            {
                DialogManager.lastDialogSceneId = autoSave.schema;
                DialogManager.lastDialogIndex = autoSave.dialogIndex;
                Debug.Log(
                    $"Continuing dialog after quiz: schema={autoSave.schema}, dialogIndex={autoSave.dialogIndex}"
                );
            }
        }

        MenuController.GetOrCreateInstance().LoadToScene("DialogScene");
    }
}
