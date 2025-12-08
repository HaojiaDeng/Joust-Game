using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance;

    [SerializeField] private Transform hpEnemyParent;
    private List<GameObject> healthSegments = new List<GameObject>();
    private int lastHealth = -1;

    private void Awake()
    {
        Instance = this;
        InitializeHealthSegments();
    }

    private void InitializeHealthSegments()
    {
        if (hpEnemyParent == null)
        {
            hpEnemyParent = GameObject.Find("HPEnemy")?.transform;
        }

        if (hpEnemyParent == null)
        {
            Debug.LogError("BossHealthUI: HPEnemy not found!");
            return;
        }

        healthSegments.Clear();
        for (int i = 0; i < hpEnemyParent.childCount; i++)
        {
            GameObject segment = hpEnemyParent.GetChild(i).gameObject;
            healthSegments.Add(segment);
            Debug.Log($"BossHealthUI: Segment {i} = {segment.name}");
        }

        Debug.Log($"BossHealthUI: Found {healthSegments.Count} health segments");
    }

    private void Update()
    {
        if (BossStats.Instance != null && BossStats.Instance.IsActive())
        {
            int currentHealth = BossStats.Instance.GetCurrentHealth();
            if (currentHealth != lastHealth)
            {
                UpdateHealth(currentHealth);
            }
        }
        else if (lastHealth != -1)
        {
            lastHealth = -1;
            ShowAllSegments();
        }
    }

    public void UpdateHealth(int currentHealth)
    {
        if (healthSegments.Count == 0)
        {
            InitializeHealthSegments();
        }

        int maxHealth = healthSegments.Count;
        int segmentsToHide = maxHealth - currentHealth;
        
        for (int i = 0; i < healthSegments.Count; i++)
        {
            int indexFromEnd = maxHealth - 1 - i;
            if (i < segmentsToHide)
            {
                healthSegments[indexFromEnd].SetActive(false);
            }
            else
            {
                healthSegments[indexFromEnd].SetActive(true);
            }
        }

        lastHealth = currentHealth;
        
        Debug.Log($"BossHealthUI: Updated to {currentHealth}/{maxHealth} health. Hidden {segmentsToHide} segments from right.");
    }

    private void ShowAllSegments()
    {
        foreach (GameObject segment in healthSegments)
        {
            segment.SetActive(true);
        }
    }
}

