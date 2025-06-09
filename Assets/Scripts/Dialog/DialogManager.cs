using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            return;
        }

        var line = currentScene.dialog[currentLine];
        speakerText.text = line.speaker;
        dialogText.text = line.text;

        UpdateCharacterVisuals(line.speaker);

        currentLine++;
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
