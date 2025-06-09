using UnityEngine;
using System.Collections.Generic;

public class VerbChallengeLoader : MonoBehaviour
{
    public static VerbChallengeLoader Instance;
    public List<VerbChallenge> challenges;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        LoadChallenges();
    }

    void LoadChallenges() {
        TextAsset json = Resources.Load<TextAsset>("Quiz/dynamic_verb_challenges");
        if (json == null) {
            Debug.LogError("dynamic_verb_challenges.json not found in Resources/Quiz");
            return;
        }
        VerbChallengeWrapper wrapper = JsonUtility.FromJson<VerbChallengeWrapper>(json.text);
        challenges = wrapper.challenges;
    }

    public VerbChallenge GetChallenge(int index) {
        if (challenges == null || index < 0 || index >= challenges.Count) return null;
        return challenges[index];
    }
}
