using UnityEngine;

public class QTEHealthManager : MonoBehaviour
{
    public static QTEHealthManager Instance;

    private bool isQTEActive = false;

    public bool IsQTEActive => isQTEActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            isQTEActive = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

