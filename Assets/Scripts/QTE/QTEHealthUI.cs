using UnityEngine;

public class QTEHealthUI : MonoBehaviour
{
    public static QTEHealthUI Instance;

    private void Awake()
    {
        Instance = this;
    }
    
    public void UpdateHealth(int currentHealth)
    {
        Debug.LogWarning($"QTEHealthUI: UpdateHealth({currentHealth}) called but QTE health system is disabled! Ignoring.");
    }
}

