using UnityEngine;

public class RetryQuizButton : MonoBehaviour
{
    public void OnRetryButtonClicked()
    {
        GameManager.RetryLastQuiz();
    }
}
