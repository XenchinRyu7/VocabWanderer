using UnityEngine;
using UnityEngine.EventSystems;

public class BlockClickHandler : MonoBehaviour, IPointerClickHandler
{
    public MenuController sceneManager; 
    public string targetSceneName = "QuestionScene";

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sceneManager != null && !string.IsNullOrEmpty(targetSceneName))
        {
            sceneManager.LoadToScene(targetSceneName);
        }
    }
}
