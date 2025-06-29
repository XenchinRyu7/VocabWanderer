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
        // Jika ada schema dari parameter save, load file JSON yang sesuai
        if (!string.IsNullOrEmpty(lastDialogSceneId))
        {
            LoadDialogFromSchema(lastDialogSceneId);
        }
        else
        {
            LoadSceneFromJson();
        }

        SetupBackground();
        SetupCharacterMap();

        // Cek jika ada posisi dialog tersimpan, lanjutkan dari sana
        if (lastDialogSceneId == sceneId && lastDialogIndex >= 0)
        {
            currentLine = lastDialogIndex;
            lastDialogSceneId = null;
            lastDialogIndex = -1;
        }
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
            Debug.Log($"Loaded dialog from schema: {schema}, sceneId: {sceneId}");
        }
        else
        {
            Debug.LogError($"Dialog file not found: {path}");
            LoadSceneFromJson();
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
        if (!string.IsNullOrEmpty(line.@event) && line.@event == "quiz")
        {
            // Simpan posisi dialog setelah quiz
            lastDialogSceneId = sceneId;
            lastDialogIndex = currentLine + 1;
            string quizSchema = line.quiz_schema;
            int quizIndex = line.quiz_index ?? 1;
            // Simpan state quiz terakhir untuk retry
            GameManager.SetLastQuiz(quizSchema, quizIndex);
            Debug.Log(
                $"[DialogManager] Navigasi ke QuestionScene: schema={quizSchema}, index={quizIndex}"
            );
            QuizNavigationParam.schema = quizSchema;
            QuizNavigationParam.index = quizIndex;
            SceneManager.LoadScene("QuestionScene");
            return;
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
            if (EventSystem.current.IsPointerOverGameObject())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition,
                };

                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.name == "PauseButton")
                    {
                        return;
                    }
                }
            }

            ShowNextLine();
        }
    }
}
