using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DynamicUIBuilder : MonoBehaviour
{
    public static DynamicUIBuilder Instance;
    public Transform letterSlotParent;
    public Transform letterTileParent;
    public GameObject letterBoxPrefab;
    public GameObject letterSlotPrefab;
    public GameObject letterTilePrefab;
    public TextMeshProUGUI contextText;
    public Image backgroundImage;
    public TimerLine timerLine;
    public HealthManager healthManager;
    public Transform healthParent;
    public GameObject heartPrefabFull;
    public GameObject heartPrefabEmpty;
    public int maxHealth = 5;

    void Awake() {
        Instance = this;
    }

    void Start()
    {
        if (timerLine != null)
        {
            timerLine.OnLineTimeout = () => {
                GameManager.Instance.OnTimeOut();
                timerLine.StartLine(GameManager.Instance.currentQuestion.time_limit_seconds);
            };
        }
    }

    public void UpdateHealthUI(int currentHealth)
    {
        foreach (Transform child in healthParent) Destroy(child.gameObject);
        for (int i = 0; i < maxHealth; i++)
        {
            Instantiate(i < currentHealth ? heartPrefabFull : heartPrefabEmpty, healthParent);
        }
    }

    public void BuildQuestionUI(VerbQuestion question, string backgroundAsset) {
        // Clear previous UI
        foreach (Transform child in letterSlotParent) Destroy(child.gameObject);
        foreach (Transform child in letterTileParent) Destroy(child.gameObject);

        // Set context
        contextText.text = question.context;

        // Set background
        Sprite bg = Resources.Load<Sprite>(backgroundAsset.Replace("assets/Resources/", "").Replace(".png", ""));
        if (bg != null && backgroundImage != null) backgroundImage.sprite = bg;

        // Build letter slots (boxes)
        string scrambled = question.scrambled_word;
        for (int i = 0; i < scrambled.Length; i++) {
            char c = scrambled[i];
            GameObject slot = Instantiate(c == '_' ? letterSlotPrefab : letterBoxPrefab, letterSlotParent);
            var text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (c != '_') text.text = c.ToString();
            else text.text = "";
        }

        List<string> tiles = new List<string>(question.missing_letters);
        while (tiles.Count < 8) {
            char rnd = (char)('A' + Random.Range(0, 26));
            if (!tiles.Contains(rnd.ToString())) tiles.Add(rnd.ToString());
        }
        // Shuffle
        for (int i = 0; i < tiles.Count; i++) {
            var tmp = tiles[i];
            int rand = Random.Range(i, tiles.Count);
            tiles[i] = tiles[rand];
            tiles[rand] = tmp;
        }
        foreach (var t in tiles) {
            GameObject tile = Instantiate(letterTilePrefab, letterTileParent);
            tile.GetComponentInChildren<TextMeshProUGUI>().text = t.ToUpper();
            tile.GetComponent<DraggableLetter>().letter = t[0];
        }
        // Sinkronkan TimerLine dengan waktu soal
        if (timerLine != null)
        {
            Debug.Log($"[DynamicUIBuilder] TimerLine.StartLine dipanggil dengan waktu: {question.time_limit_seconds}");
            timerLine.StartLine(question.time_limit_seconds);
            timerLine.OnLineTimeout = () => {
                Debug.Log("[DynamicUIBuilder] TimerLine.OnLineTimeout trigger, memanggil GameManager.OnTimeOut()");
                GameManager.Instance.OnTimeOut();
                // Cek jika health masih > 0, reset timerLine untuk soal yang sama
                if (GameManager.Instance.health > 0)
                {
                    Debug.Log($"[DynamicUIBuilder] TimerLine direset dengan waktu: {question.time_limit_seconds}");
                    timerLine.StartLine(question.time_limit_seconds);
                }
            };
        }
        else
        {
            Debug.LogWarning("TimerLine belum di-assign di DynamicUIBuilder!");
        }
        // Sinkronkan HealthManager dengan health saat build UI
        if (healthManager != null && GameManager.Instance != null)
        {
            healthManager.UpdateHealth(GameManager.Instance.health);
        }
        else
        {
            Debug.LogWarning("HealthManager atau GameManager.Instance belum di-assign di DynamicUIBuilder!");
        }
        // Update health UI setelah build soal
        if (healthParent != null && heartPrefabFull != null && heartPrefabEmpty != null && GameManager.Instance != null)
        {
            UpdateHealthUI(GameManager.Instance.health);
        }
        else
        {
            Debug.LogWarning("Health UI prefab/parent/GameManager belum di-assign di DynamicUIBuilder!");
        }
    }
}
