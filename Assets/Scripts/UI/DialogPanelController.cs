using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogPanelController : MonoBehaviour
{
    [Header("Dialog Buttons")]
    public Button continueButton;
    public Button saveButton;
    public Button exitButton;

    void Start()
    {
        SetupDialogPanel();
    }

    void SetupDialogPanel()
    {
        // Sembunyikan button Save jika di QuestionScene
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "QuestionScene")
        {
            if (saveButton != null)
            {
                saveButton.gameObject.SetActive(false);
                Debug.Log("DialogPanelController: Save button hidden in QuestionScene");
            }
        }
        else
        {
            if (saveButton != null)
            {
                saveButton.gameObject.SetActive(true);
                Debug.Log("DialogPanelController: Save button visible in " + currentScene);
            }
        }

        // Setup button listeners
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }
    }

    public void onHomeClicked()
    {
        Debug.Log("DialogPanelController: Home button clicked");

        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.LoadToScene("MainMenu");
        }
        else
        {
            Debug.LogError("DialogPanelController: MenuController not found!");
        }
    }

    public void OnContinueClicked()
    {
        Debug.Log("DialogPanelController: Continue button clicked");

        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.HideDialog();
        }
        else
        {
            Debug.LogError("DialogPanelController: MenuController not found!");
        }
    }

    public void OnSaveClicked()
    {
        Debug.Log("DialogPanelController: Save button clicked");

        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.showSaveDialog();
        }
        else
        {
            Debug.LogError("DialogPanelController: MenuController not found!");
        }
    }

    public void OnExitClicked()
    {
        Debug.Log("DialogPanelController: Exit button clicked");

        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.ExitGame();
        }
        else
        {
            Debug.LogError("DialogPanelController: MenuController not found!");
            Application.Quit();
        }
    }
}
