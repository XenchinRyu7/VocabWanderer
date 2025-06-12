using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json; // Tambahkan ini

public class VerbChallengeLoader : MonoBehaviour
{
    public static VerbChallengeLoader Instance;
    public List<VerbChallenge> challenges;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void LoadChallenges(string schema)
    {
        string path = $"Quiz/quiz_{schema}"; 
        TextAsset json = Resources.Load<TextAsset>(path);
        if (json == null) {
            Debug.LogError($"{path} not found in Resources/Quiz");
            challenges = null;
            return;
        }
        challenges = JsonConvert.DeserializeObject<List<VerbChallenge>>(json.text);
    }

    public VerbChallenge GetChallenge(int index) {
        if (challenges == null || index < 0 || index >= challenges.Count) return null;
        return challenges[index];
    }
}
