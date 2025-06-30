using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public GameObject backgroundGO;

    public GameObject leftCharacterGO;
    public GameObject rightCharacterGO;

    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogText;

    public TextAsset dialogJson;
    public string sceneId; // Set ini di inspector sesuai sceneId di json

    private DialogScene currentScene;
    private int currentLine = 0;

    private Dictionary<string, CharacterInfo> characterMap =
        new Dictionary<string, CharacterInfo>();

    public GameObject clickToContinueGO;

    public string nextSceneName = "QuestionScene";
    public GameObject nextButton;

    // Untuk melanjutkan dialog setelah quiz
    public static string lastDialogSceneId = null;
    public static int lastDialogIndex = -1;

    void Start()
    {
        Debug.Log("=== DialogManager.Start() Begin ===");

        // Cek dan gunakan SaveManager untuk memuat progress dari autosave
        SaveManager saveManager = SaveManager.EnsureInstance();
        bool loadedFromSave = false;

        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            Debug.Log(
                $"AutoSave data: schema={autoSave?.schema}, dialogIndex={autoSave?.dialogIndex}"
            );

            if (autoSave != null && !string.IsNullOrEmpty(autoSave.schema))
            {
                Debug.Log(
                    $"Loading from autosave: schema={autoSave.schema}, dialogIndex={autoSave.dialogIndex}"
                );
                LoadDialogFromSchema(autoSave.schema);
                currentLine = autoSave.dialogIndex;
                loadedFromSave = true;
                Debug.Log($"Set currentLine to: {currentLine}");
            }
        }

        // Fallback: Jika ada schema dari parameter static save, load file JSON yang sesuai
        if (!loadedFromSave && !string.IsNullOrEmpty(lastDialogSceneId))
        {
            Debug.Log(
                $"Loading from static params: lastDialogSceneId={lastDialogSceneId}, lastDialogIndex={lastDialogIndex}"
            );
            LoadDialogFromSchema(lastDialogSceneId);
            if (lastDialogIndex >= 0)
            {
                currentLine = lastDialogIndex;
                Debug.Log($"Set currentLine from static to: {currentLine}");
            }
            lastDialogSceneId = null;
            lastDialogIndex = -1;
        }
        // Fallback terakhir: gunakan dialogJson default
        else if (!loadedFromSave)
        {
            Debug.Log("Loading from default dialogJson");
            LoadSceneFromJson();
        }

        SetupBackground();
        SetupCharacterMap();

        Debug.Log($"=== DialogManager.Start() End - currentLine={currentLine} ===");
        ShowNextLine();
    }

    void LoadSceneFromJson()
    {
        currentScene = JsonConvert.DeserializeObject<DialogScene>(dialogJson.text);
    }

    void LoadDialogFromSchema(string schema)
    {
        string path = $"Dialog/dialog_{schema}";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile != null)
        {
            currentScene = JsonConvert.DeserializeObject<DialogScene>(jsonFile.text);
            sceneId = currentScene.sceneId;

            // Pastikan field schema terisi, jika tidak ada di JSON, set dari parameter
            if (string.IsNullOrEmpty(currentScene.schema))
            {
                currentScene.schema = schema;
            }

            Debug.Log(
                $"Loaded dialog from schema: {schema}, sceneId: {sceneId}, currentScene.schema: {currentScene.schema}"
            );
        }
        else
        {
            Debug.LogError($"Dialog file not found: {path}");
            // Fallback ke dialogJson default jika ada
            if (dialogJson != null)
            {
                Debug.Log("Falling back to default dialogJson");
                LoadSceneFromJson();
                // Set schema secara manual untuk fallback
                if (currentScene != null)
                {
                    currentScene.schema = schema;
                }
            }
            else
            {
                Debug.LogError("No fallback dialogJson available!");
            }
        }
    }

    void SetupBackground()
    {
        Image img = backgroundGO.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("[DialogManager] Komponen Image tidak ditemukan di backgroundGO!");
            return;
        }
        string bgPath = "Background/" + currentScene.background;
        Debug.Log($"[DialogManager] Mencoba load background dari path: {bgPath}");
        Sprite bgSprite = Resources.Load<Sprite>(bgPath);
        if (bgSprite == null)
        {
            Debug.LogError($"[DialogManager] Sprite background tidak ditemukan di path: {bgPath}");
        }
        else
        {
            Debug.Log($"[DialogManager] Sprite background berhasil di-load: {bgSprite.name}");
            img.sprite = bgSprite;
        }
    }

    void SetupCharacterMap()
    {
        foreach (var character in currentScene.characters)
        {
            characterMap[character.name] = character;
        }
    }

    public void ShowNextLine()
    {
        if (currentLine >= currentScene.dialog.Count)
        {
            Debug.Log("Dialog selesai.");

            if (clickToContinueGO != null)
                clickToContinueGO.SetActive(false);
            if (nextButton != null)
            {
                nextButton.SetActive(true);
            }
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            return;
        }

        var line = currentScene.dialog[currentLine];
        if (!string.IsNullOrEmpty(line.@event))
        {
            if (line.@event == "quiz")
            {
                // Simpan posisi dialog setelah quiz untuk static fallback
                lastDialogSceneId = sceneId;
                lastDialogIndex = currentLine + 1;

                string quizSchema = line.quiz_schema;
                int quizIndex = line.quiz_index ?? 1;

                GameManager.SetLastQuiz(quizSchema, quizIndex);
                Debug.Log(
                    $"[DialogManager] Navigasi ke QuestionScene: schema={quizSchema}, index={quizIndex}"
                );
                QuizNavigationParam.schema = quizSchema;
                QuizNavigationParam.index = quizIndex;
                SceneManager.LoadScene("QuestionScene");
                return;
            }
            else if (line.@event == "next_schema")
            {
                // Berpindah ke schema berikutnya
                string nextSchema = line.next_schema;
                if (!string.IsNullOrEmpty(nextSchema))
                {
                    Debug.Log($"[DialogManager] Moving to next schema: {nextSchema}");

                    // Auto-save progress ke schema baru dengan dialogIndex 0
                    SaveManager saveManager = SaveManager.EnsureInstance();
                    if (saveManager != null)
                    {
                        saveManager.AutoSaveProgress(nextSchema, 0, 0);
                        Debug.Log($"Auto-saved transition to: {nextSchema}");
                    }

                    // Set static variables untuk load schema baru
                    lastDialogSceneId = nextSchema;
                    lastDialogIndex = 0;

                    // Reload scene dengan schema baru
                    SceneManager.LoadScene("DialogScene");
                    return;
                }
                else
                {
                    Debug.LogError("next_schema event found but no next_schema specified!");
                }
            }
            else if (line.@event == "game_complete")
            {
                // Game selesai, kembali ke main menu atau ending screen
                Debug.Log("[DialogManager] Game completed!");

                // Reset auto-save atau save final completion
                SaveManager saveManager = SaveManager.EnsureInstance();
                if (saveManager != null)
                {
                    saveManager.AutoSaveProgress("game_completed", 0, 0);
                    Debug.Log("Game completion saved!");
                }

                // Load main menu atau ending scene
                SceneManager.LoadScene("MainMenu");
                return;
            }
        }

        speakerText.text = line.speaker;
        dialogText.text = line.text;

        UpdateCharacterVisuals(line.speaker);

        currentLine++;

        if (clickToContinueGO != null)
            clickToContinueGO.SetActive(true);
        if (nextButton != null)
            nextButton.SetActive(false);
    }

    void UpdateCharacterVisuals(string currentSpeaker)
    {
        if (!characterMap.ContainsKey(currentSpeaker))
        {
            Debug.LogWarning("Character not found: " + currentSpeaker);
            return;
        }

        var speakerCharacter = characterMap[currentSpeaker];
        var speakerSprite = Resources.Load<Sprite>("Characters/" + speakerCharacter.sprite);
        string speakerPos = speakerCharacter.position.ToLower();

        if (speakerPos == "left")
        {
            var img = leftCharacterGO.GetComponent<Image>();
            img.sprite = speakerSprite;
            leftCharacterGO.SetActive(true);
        }
        else
        {
            var img = rightCharacterGO.GetComponent<Image>();
            img.sprite = speakerSprite;
            rightCharacterGO.SetActive(true);
        }

        var supportCharacter = currentScene.characters.FirstOrDefault(c =>
            c.name != currentSpeaker
        );

        if (supportCharacter != null)
        {
            var c = supportCharacter;
            var supportSprite = Resources.Load<Sprite>("Characters/" + c.sprite);
            string supportPos = c.position.ToLower();

            if (supportPos == speakerPos)
                supportPos = (speakerPos == "left") ? "right" : "left";

            if (supportPos == "left")
            {
                var img = leftCharacterGO.GetComponent<Image>();
                img.sprite = supportSprite;
                leftCharacterGO.SetActive(true);
            }
            else
            {
                var img = rightCharacterGO.GetComponent<Image>();
                img.sprite = supportSprite;
                rightCharacterGO.SetActive(true);
            }
        }
        else
        {
            if (speakerPos == "left")
                rightCharacterGO.SetActive(false);
            else
                leftCharacterGO.SetActive(false);
        }
    }

    public void OnClickNext()
    {
        ShowNextLine();
    }

    public void OnClickNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if click is over any UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition,
                };

                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                // Check if click is on any UI element that should block dialog progression
                foreach (var result in results)
                {
                    // Skip ShowNextLine if clicking on buttons or UI panels
                    if (
                        result.gameObject.name == "PauseButton"
                        || result.gameObject.name.Contains("Button")
                        || result.gameObject.name.Contains("Panel")
                        || result.gameObject.name.Contains("SaveMenu")
                        || result.gameObject.name.Contains("Dialog")
                    )
                    {
                        Debug.Log($"Click blocked by UI element: {result.gameObject.name}");
                        return;
                    }
                }

                Debug.Log("Click on UI but no blocking elements found, continuing dialog");
            }

            ShowNextLine();
        }
    }

    private string GetCurrentSchema()
    {
        SaveManager saveManager = SaveManager.EnsureInstance();
        if (saveManager != null)
        {
            var autoSave = saveManager.GetCurrentAutoSave();
            if (autoSave != null && !string.IsNullOrEmpty(autoSave.schema))
            {
                Debug.Log($"GetCurrentSchema: Found from autoSave: {autoSave.schema}");
                return autoSave.schema;
            }
        }

        // Cek apakah currentScene memiliki field schema yang sudah di-set
        if (currentScene != null && !string.IsNullOrEmpty(currentScene.schema))
        {
            Debug.Log($"GetCurrentSchema: Found from currentScene: {currentScene.schema}");
            return currentScene.schema;
        }

        if (!string.IsNullOrEmpty(lastDialogSceneId))
        {
            Debug.Log($"GetCurrentSchema: Using lastDialogSceneId: {lastDialogSceneId}");
            return lastDialogSceneId;
        }

        Debug.Log("GetCurrentSchema: Using default schema_1");
        return "schema_1";
    }

    // Helper method untuk ekstrak schema dari filename
    private string ExtractSchemaFromFilename(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return "schema_1";

        // Jika filename mengandung "dialog_schema_X", ekstrak "schema_X"
        if (filename.Contains("dialog_schema_"))
        {
            int startIndex = filename.IndexOf("dialog_schema_") + "dialog_".Length;
            int endIndex = filename.IndexOf('.', startIndex);
            if (endIndex == -1)
                endIndex = filename.Length;

            return filename.Substring(startIndex, endIndex - startIndex);
        }

        return "schema_1"; // Default fallback
    }
}
