using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int currentChallengeIndex = 0;
    public int currentQuestionIndex = 0;
    public int health = 5;
    public VerbChallenge currentChallenge;
    public VerbQuestion currentQuestion;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        LoadChallenge(currentChallengeIndex);
        LoadQuestion(0);
    }

    public void LoadChallenge(int index) {
        Debug.Log($"LoadChallenge dipanggil dengan index={index}");
        if (VerbChallengeLoader.Instance == null) {
            Debug.LogError("VerbChallengeLoader.Instance is null di LoadChallenge!");
            return;
        }
        Debug.Log("Sebelum GetChallenge");
        currentChallenge = VerbChallengeLoader.Instance.GetChallenge(index);
        Debug.Log("Setelah GetChallenge");
        if (currentChallenge == null) {
            Debug.LogError($"GetChallenge({index}) return null di LoadChallenge!");
            return;
        }
        currentQuestionIndex = 0;
    }

    public void LoadQuestion(int index) {
        currentQuestionIndex = index;
        if (currentChallenge == null) {
            Debug.LogError("currentChallenge is null di LoadQuestion!");
            return;
        }
        if (currentChallenge.questions == null) {
            Debug.LogError("currentChallenge.questions is null di LoadQuestion!");
            return;
        }
        if (index < 0 || index >= currentChallenge.questions.Count) {
            Debug.LogError($"Index {index} out of range di LoadQuestion! questions.Count={currentChallenge.questions.Count}");
            return;
        }
        currentQuestion = currentChallenge.questions[index];
        if (currentQuestion == null) {
            Debug.LogError($"currentQuestion null di LoadQuestion! index={index}");
            return;
        }
        Debug.Log("currentQuestion berhasil diambil, lanjut ke BuildQuestionUI");
        if (DynamicUIBuilder.Instance == null) {
            Debug.LogError("DynamicUIBuilder.Instance null di LoadQuestion!");
            return;
        }
        DynamicUIBuilder.Instance.BuildQuestionUI(currentQuestion, currentChallenge.background_asset);
    }

    public void OnTimeOut() {
        health--;
        // Update health UI secara dinamis
        if (DynamicUIBuilder.Instance != null)
            DynamicUIBuilder.Instance.UpdateHealthUI(health);
        if (health <= 0)
            SceneManager.LoadScene("GameOver");
        else
            LoadQuestion(currentQuestionIndex);
    }

    public void OnAnswerSubmitted(bool correct) {
        if (correct) {
            if (currentQuestionIndex < currentChallenge.questions.Count - 1)
                LoadQuestion(currentQuestionIndex + 1);
            else
                SceneManager.LoadScene("SuccessScene");
        } else {
            health--;
            if (DynamicUIBuilder.Instance != null)
                DynamicUIBuilder.Instance.UpdateHealthUI(health);
            if (health <= 0)
                SceneManager.LoadScene("GameOver");
            else
                LoadQuestion(currentQuestionIndex);
        }
    }
}

