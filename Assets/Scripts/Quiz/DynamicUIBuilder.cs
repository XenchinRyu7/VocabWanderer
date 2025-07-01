using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicUIBuilder : MonoBehaviour
{
    public Transform letterSlotParent;
    public Transform letterTileParent;
    public GameObject letterBoxPrefab;
    public GameObject letterSlotPrefab;
    public GameObject letterTilePrefab;
    public TextMeshProUGUI contextText;
    public Image backgroundImage;
    public TimerLine timerLine;
    public Transform healthParent;
    public GameObject heartPrefabFull;
    public GameObject heartPrefabEmpty;
    public int maxHealth = 5;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateHealthUI(GameManager.Instance.health);
        }
        else
        {
            UpdateHealthUI(maxHealth);
        }

        if (timerLine != null)
        {
            timerLine.OnLineTimeout = () =>
            {
                GameManager.Instance.OnTimeOut();
                timerLine.StartLine(GameManager.Instance.currentQuestion.time_limit_seconds);
            };
            if (GameManager.Instance != null && GameManager.Instance.currentQuestion != null)
            {
                timerLine.StartLine(GameManager.Instance.currentQuestion.time_limit_seconds);
            }
        }
    }

    public void UpdateHealthUI(int currentHealth)
    {
        foreach (Transform child in healthParent)
            Destroy(child.gameObject);
        for (int i = 0; i < maxHealth; i++)
        {
            Instantiate(i < currentHealth ? heartPrefabFull : heartPrefabEmpty, healthParent);
        }
    }

    public void BuildQuestionUI(VerbQuestion question, string backgroundAsset)
    {
        foreach (Transform child in letterSlotParent)
            Destroy(child.gameObject);
        foreach (Transform child in letterTileParent)
            Destroy(child.gameObject);

        contextText.text = question.context;

        Sprite bg = Resources.Load<Sprite>(
            backgroundAsset.Replace("assets/Resources/", "").Replace(".png", "")
        );
        if (bg != null && backgroundImage != null)
            backgroundImage.sprite = bg;

        string scrambled = question.scrambled_word;
        for (int i = 0; i < scrambled.Length; i++)
        {
            char c = scrambled[i];
            GameObject slot = Instantiate(
                c == '_' ? letterSlotPrefab : letterBoxPrefab,
                letterSlotParent
            );
            var text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (c != '_')
                text.text = c.ToString();
            else
                text.text = "";
        }

        List<string> tiles = new List<string>(question.missing_letters);
        while (tiles.Count < 8)
        {
            char rnd = (char)('A' + Random.Range(0, 26));
            if (!tiles.Contains(rnd.ToString()))
                tiles.Add(rnd.ToString());
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            var tmp = tiles[i];
            int rand = Random.Range(i, tiles.Count);
            tiles[i] = tiles[rand];
            tiles[rand] = tmp;
        }
        foreach (var t in tiles)
        {
            GameObject tile = Instantiate(letterTilePrefab, letterTileParent);
            tile.GetComponentInChildren<TextMeshProUGUI>().text = t.ToUpper();
            tile.GetComponent<DraggableLetter>().letter = t[0];
        }
        if (timerLine != null)
        {
            Debug.Log(
                $"[DynamicUIBuilder] TimerLine.StartLine dipanggil dengan waktu: {question.time_limit_seconds}"
            );
            timerLine.StartLine(question.time_limit_seconds);
            timerLine.OnLineTimeout = () =>
            {
                Debug.Log(
                    "[DynamicUIBuilder] TimerLine.OnLineTimeout trigger, memanggil GameManager.OnTimeOut()"
                );
                GameManager.Instance.OnTimeOut();
                if (GameManager.Instance.health > 0)
                {
                    Debug.Log(
                        $"[DynamicUIBuilder] TimerLine direset dengan waktu: {question.time_limit_seconds}"
                    );
                    timerLine.StartLine(question.time_limit_seconds);
                }
            };
            StartCoroutine(CheckTimeAlmostOutCoroutine(question.time_limit_seconds));
        }
    }

    public void PauseGame()
    {
        try
        {
            // Find MenuController in current scene
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController != null)
            {
                Debug.Log("PauseGame: Found MenuController, calling ShowDialog()");
                menuController.ShowDialog();
            }
            else
            {
                Debug.LogError("PauseGame: MenuController not found in current scene!");
            }
            if (timerLine != null)
                timerLine.Pause();
            if (GameManager.Instance != null)
                GameManager.Instance.PauseTotalTime();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in PauseGame: {e.Message}");
            return;
        }
    }

    public void ResumeGame()
    {
        try
        {
            // Find MenuController in current scene
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController != null)
            {
                Debug.Log("ResumeGame: Found MenuController, calling HideDialog()");
                menuController.HideDialog();
            }
            else
            {
                Debug.LogError("ResumeGame: MenuController not found in current scene!");
                Debug.Log("ResumeGame: Fallback - resuming timer and game directly");

                if (timerLine != null)
                    timerLine.Resume();
                if (GameManager.Instance != null)
                    GameManager.Instance.ResumeTotalTime();

                Destroy(gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ResumeGame: {e.Message}");

            if (timerLine != null)
                timerLine.Resume();
            if (GameManager.Instance != null)
                GameManager.Instance.ResumeTotalTime();
        }
    }

    private System.Collections.IEnumerator CheckTimeAlmostOutCoroutine(float totalTime)
    {
        float threshold = 5f; // detik sebelum habis
        bool called = false;
        while (timerLine != null && timerLine.RemainingTime > 0)
        {
            if (!called && timerLine.RemainingTime <= threshold)
            {
                called = true;
                if (BacksoundPlayer.instance != null)
                    BacksoundPlayer.instance.PlayTimeAlmostOutSound();
            }
            yield return null;
        }
    }
}
