using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;
    public Transform heartParent;
    public GameObject heartPrefabFull;
    public GameObject heartPrefabEmpty;
    public int maxHealth = 5;

    void Awake() { Instance = this; }

    public void UpdateHealth(int health)
    {
        foreach (Transform child in heartParent) Destroy(child.gameObject);
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(i < health ? heartPrefabFull : heartPrefabEmpty, heartParent);
        }
    }
}
