using UnityEngine;

public static class QuizResult
{
    public static int lastHealth;
    public static float lastTotalTime;
    public static bool hasData = false;

    public static void SetResult(int health, float totalTime)
    {
        lastHealth = health;
        lastTotalTime = totalTime;
        hasData = true;
        Debug.Log($"[QuizResult] Stored: health={health}, totalTime={totalTime}");
    }

    public static void Clear()
    {
        lastHealth = 0;
        lastTotalTime = 0f;
        hasData = false;
        Debug.Log("[QuizResult] Data cleared");
    }
}
