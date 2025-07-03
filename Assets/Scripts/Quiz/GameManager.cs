using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int lastHealth;
    public static float lastTotalTime;
    public static string lastQuizSchema;
    public static int lastQuizIndex;
    public int currentChallengeIndex = 0;
    public int currentQuestionIndex = 0;
    public int health = 5;
    public VerbChallenge currentChallenge;
    public VerbQuestion currentQuestion;
    public float totalPlayTime = 0f;
    private bool isPaused = false;

    void Awake()
    {
        Debug.Log("[GameManager] Awake called");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[GameManager] Set as singleton instance");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("[GameManager] Duplicate instance found, destroying this duplicate");
            Debug.Log("[GameManager] Triggering quiz setup on existing instance");
            // Trigger quiz setup pada instance yang sudah ada
            Instance.SetupQuizFromScene();
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hanya jalankan jika ini adalah instance pertama (bukan duplicate)
        if (Instance == this)
        {
            SetupQuizFromScene();
        }
    }

    public void SetupQuizFromScene()
    {
        Debug.Log(
            $"[GameManager] SetupQuizFromScene: schema={QuizNavigationParam.schema}, index={QuizNavigationParam.index}"
        );
        string schema = QuizNavigationParam.schema;
        int idx = QuizNavigationParam.index;

        // Set the last quiz schema and index for auto save
        lastQuizSchema = schema;
        lastQuizIndex = idx;

        Debug.Log(
            $"[GameManager] Set lastQuizSchema={lastQuizSchema}, lastQuizIndex={lastQuizIndex}"
        );

        // Reset state saat mulai quiz baru
        health = 5;
        totalPlayTime = 0f;
        currentChallengeIndex = idx - 1;
        currentQuestionIndex = 0;
        VerbChallengeLoader.Instance.LoadChallenges(schema);
        Debug.Log($"[GameManager] LoadChallenge index={currentChallengeIndex}");
        LoadChallenge(currentChallengeIndex);
        LoadQuestion(0);
    }

    void Update()
    {
        if (!isPaused)
        {
            totalPlayTime += Time.unscaledDeltaTime;
        }
    }

    public void PauseTotalTime()
    {
        isPaused = true;
    }

    public void ResumeTotalTime()
    {
        isPaused = false;
    }

    public void LoadChallenge(int index)
    {
        Debug.Log($"[GameManager] LoadChallenge called with index={index}");
        if (VerbChallengeLoader.Instance == null)
        {
            Debug.LogError("VerbChallengeLoader.Instance is null di LoadChallenge!");
            return;
        }
        Debug.Log("[GameManager] VerbChallengeLoader found, checking total challenges");
        int totalChallenges = VerbChallengeLoader.Instance.challenges?.Count ?? 0;
        Debug.Log($"[GameManager] Total challenges available: {totalChallenges}");
        Debug.Log($"[GameManager] Requesting challenge at index: {index}");

        if (index >= totalChallenges || totalChallenges == 0)
        {
            Debug.LogError(
                $"[GameManager] Challenge index {index} out of range! Total challenges: {totalChallenges}"
            );
            return;
        }

        currentChallenge = VerbChallengeLoader.Instance.GetChallenge(index);
        Debug.Log(
            $"[GameManager] GetChallenge returned: {(currentChallenge != null ? "SUCCESS" : "NULL")}"
        );
        if (currentChallenge == null)
        {
            Debug.LogError($"GetChallenge({index}) return null di LoadChallenge!");
            return;
        }
        Debug.Log(
            $"[GameManager] Challenge loaded successfully: questions count = {currentChallenge.questions?.Count ?? 0}"
        );
        currentQuestionIndex = 0;
    }

    public void LoadQuestion(int index)
    {
        currentQuestionIndex = index;
        if (currentChallenge == null)
        {
            Debug.LogError("currentChallenge is null di LoadQuestion!");
            return;
        }
        if (currentChallenge.questions == null)
        {
            Debug.LogError("currentChallenge.questions is null di LoadQuestion!");
            return;
        }
        Debug.Log($"[GameManager] LoadQuestion called with index={index}");
        Debug.Log($"[GameManager] currentChallenge is null: {currentChallenge == null}");
        if (currentChallenge != null)
        {
            Debug.Log(
                $"[GameManager] currentChallenge.questions.Count={currentChallenge.questions.Count}"
            );
        }

        if (index < 0 || index >= currentChallenge.questions.Count)
        {
            Debug.LogError(
                $"Index {index} out of range di LoadQuestion! questions.Count={currentChallenge.questions.Count}"
            );
            return;
        }
        currentQuestion = currentChallenge.questions[index];
        if (currentQuestion == null)
        {
            Debug.LogError($"currentQuestion null di LoadQuestion! index={index}");
            return;
        }
        Debug.Log(
            $"[GameManager] currentQuestion berhasil diambil: {currentQuestion.answer}, lanjut ke BuildQuestionUI"
        );
        DynamicUIBuilder dynamicUIBuilder = FindObjectOfType<DynamicUIBuilder>();
        if (dynamicUIBuilder == null)
        {
            Debug.LogError("DynamicUIBuilder not found in current scene in LoadQuestion!");
            return;
        }
        Debug.Log("[GameManager] DynamicUIBuilder found, calling BuildQuestionUI");
        dynamicUIBuilder.BuildQuestionUI(currentQuestion, currentChallenge.background_asset);
    }

    public void OnTimeOut()
    {
        health--;
        DynamicUIBuilder dynamicUIBuilder = FindObjectOfType<DynamicUIBuilder>();
        if (dynamicUIBuilder != null)
            dynamicUIBuilder.UpdateHealthUI(health);
        if (health <= 0)
        {
            SceneManager.LoadScene("GameOver");
            BacksoundPlayer.instance.PlayGameOverSound();
        }
        else
            LoadQuestion(currentQuestionIndex);
    }

    public void OnAnswerSubmitted(bool correct)
    {
        if (correct)
        {
            BacksoundPlayer.instance.PlayCorrectSound();
            if (currentQuestionIndex < currentChallenge.questions.Count - 1)
                LoadQuestion(currentQuestionIndex + 1);
            else
            {
                lastHealth = health;
                lastTotalTime = totalPlayTime;

                Debug.Log($"Quiz completed. Preparing auto save...");
                Debug.Log(
                    $"Current values: lastQuizSchema='{lastQuizSchema}', lastQuizIndex={lastQuizIndex}"
                );
                Debug.Log($"DialogManager.lastDialogIndex={DialogManager.lastDialogIndex}");

                // Ensure SaveManager exists
                SaveManager saveManager = SaveManager.EnsureInstance();
                Debug.Log(
                    $"SaveManager after EnsureInstance: exists={saveManager != null}, instance match={saveManager == SaveManager.Instance}"
                );

                if (saveManager == null)
                {
                    Debug.LogError("CRITICAL: SaveManager.EnsureInstance() returned null!");
                    SceneManager.LoadScene("SuccessAllScenes");
                    return;
                }

                if (string.IsNullOrEmpty(lastQuizSchema))
                {
                    Debug.LogError(
                        $"Cannot auto save: lastQuizSchema is empty. QuizNavigationParam.schema='{QuizNavigationParam.schema}'"
                    );
                    // Try to use QuizNavigationParam.schema as fallback
                    if (!string.IsNullOrEmpty(QuizNavigationParam.schema))
                    {
                        lastQuizSchema = QuizNavigationParam.schema;
                        Debug.Log(
                            $"Using QuizNavigationParam.schema as fallback: '{lastQuizSchema}'"
                        );
                    }
                }

                if (!string.IsNullOrEmpty(lastQuizSchema))
                {
                    int dialogIndex =
                        DialogManager.lastDialogIndex > 0 ? DialogManager.lastDialogIndex : 0;
                    Debug.Log(
                        $"Calling auto save with: schema='{lastQuizSchema}', dialogIndex={dialogIndex}, quizIndex={lastQuizIndex}"
                    );

                    try
                    {
                        saveManager.UpdateProgress(lastQuizSchema, dialogIndex, lastQuizIndex);
                        Debug.Log("Auto save completed successfully!");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Auto save failed with exception: {e.Message}");
                        Debug.LogError($"Stack trace: {e.StackTrace}");
                    }
                }
                else
                {
                    Debug.LogError(
                        $"Cannot auto save: No valid schema available. lastQuizSchema='{lastQuizSchema}', QuizNavigationParam.schema='{QuizNavigationParam.schema}'"
                    );
                }

                SceneManager.LoadScene("SuccessAllScenes");
            }
        }
        else
        {
            health--;
            BacksoundPlayer.instance.PlayWrongSound();
            DynamicUIBuilder dynamicUIBuilder = FindObjectOfType<DynamicUIBuilder>();
            if (dynamicUIBuilder != null)
                dynamicUIBuilder.UpdateHealthUI(health);
            if (health <= 0)
            {
                SceneManager.LoadScene("GameOver");
                BacksoundPlayer.instance.PlayGameOverSound();
            }
            else
                LoadQuestion(currentQuestionIndex);
        }
    }

    public void ResetQuizState()
    {
        health = 5;
        totalPlayTime = 0f;
        currentChallengeIndex = 0;
        currentQuestionIndex = 0;
        currentChallenge = null;
        currentQuestion = null;
        isPaused = false;
    }

    public static void SetLastQuiz(string schema, int index)
    {
        lastQuizSchema = schema;
        lastQuizIndex = index;
    }

    public static void RetryLastQuiz()
    {
        if (!string.IsNullOrEmpty(lastQuizSchema))
        {
            if (Instance != null)
            {
                Instance.ResetQuizState();
            }
            QuizNavigationParam.schema = lastQuizSchema;
            QuizNavigationParam.index = lastQuizIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene("QuestionScene");
        }
        else
        {
            Debug.LogError("No last quiz state to retry!");
        }
    }
}
