using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogManager : MonoBehaviour
{
    public GameObject backgroundGO;

    public GameObject leftCharacterGO;
    public GameObject rightCharacterGO;

    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogText;

    public TextAsset dialogJson;

    private DialogScene currentScene;
    private int currentLine = 0;

    private Dictionary<string, CharacterInfo> characterMap = new Dictionary<string, CharacterInfo>();

    public GameObject clickToContinueGO;

    public string nextSceneName = "QuestionScene";
    public GameObject nextButton; 

    void Start()
    {
        LoadSceneFromJson();
        SetupBackground();
        SetupCharacterMap();
        ShowNextLine();
    }

    void LoadSceneFromJson()
    {
        currentScene = JsonConvert.DeserializeObject<DialogScene>(dialogJson.text);
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
                clickToContinueGO.SetActive(false); // Sembunyikan click to continue saat dialog habis
            if (nextButton != null)
            {
                nextButton.SetActive(true); // Tampilkan tombol lanjut jika ada
            }
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName); // Langsung pindah scene jika tidak pakai tombol
            }
            return;
        }

        var line = currentScene.dialog[currentLine];
        // Cek event quiz
        if (!string.IsNullOrEmpty(line.@event) && line.@event == "quiz")
        {
            string quizSchema = line.quiz_schema;
            int quizIndex = line.quiz_index ?? 1; // default ke 1 jika null
            Debug.Log($"[DialogManager] Navigasi ke QuestionScene: schema={quizSchema}, index={quizIndex}");
            // Simpan parameter ke static helper agar bisa diakses di QuestionScene
            QuizNavigationParam.schema = quizSchema;
            QuizNavigationParam.index = quizIndex;
            SceneManager.LoadScene("QuestionScene");
            return;
        }

        speakerText.text = line.speaker;
        dialogText.text = line.text;

        UpdateCharacterVisuals(line.speaker);

        currentLine++;

        // Tampilkan click to continue jika masih ada dialog berikutnya
        if (clickToContinueGO != null)
            clickToContinueGO.SetActive(true);
        if (nextButton != null)
            nextButton.SetActive(false); // Pastikan next button tetap hidden selama dialog belum habis
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

        // Pasang speaker
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

        // Ambil karakter pendukung pertama yang bukan speaker
        var supportCharacter = currentScene.characters
            .FirstOrDefault(c => c.name != currentSpeaker);

        if (supportCharacter != null)
        {
            var c = supportCharacter;
            var supportSprite = Resources.Load<Sprite>("Characters/" + c.sprite);
            string supportPos = c.position.ToLower();

            // Kalau posisinya sama dengan speaker, pindah sisi
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
            // Tidak ada karakter lain
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

    // Fungsi untuk dipanggil tombol lanjut (jika pakai tombol)
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
            // Cek apakah tap-nya kena UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // Cek apakah yang ditap adalah PauseButton
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.name == "PauseButton")
                    {
                        // Kalau tap di PauseButton, jangan lanjut
                        return;
                    }
                }
            }

            // Kalau bukan tap di PauseButton, lanjut
            ShowNextLine();
        }
    }

}
