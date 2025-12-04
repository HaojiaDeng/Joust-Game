using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public static PlayerHealthUI Instance;

    [SerializeField] private Transform hpPlayerParent;
    private List<GameObject> healthSegments = new List<GameObject>();
    private int lastHealth = -1;

    [Header("Keyboard Test Controls")]
    public KeyCode damageKey = KeyCode.Q;
    public KeyCode healKey = KeyCode.E;
    public KeyCode resetKey = KeyCode.T;
    public KeyCode initKey = KeyCode.P;

    private int testHealth = 10;
    private int testMaxHealth = 10;

    private void Awake()
    {
        Instance = this;
        InitializeHealthSegments();
    }

    private void Start()
    {
        if (GameLoopManager.Instance != null)
        {
            testHealth = GameLoopManager.Instance.currentHealth;
            testMaxHealth = GameLoopManager.Instance.maxHealth;
        }
        Debug.Log("PlayerHealthUI: Script started! Ready for testing. Press P to initialize, Q to damage, E to heal, T to reset.");
    }

    private void InitializeHealthSegments()
    {
        if (hpPlayerParent == null)
        {
            hpPlayerParent = GameObject.Find("HPPlayer")?.transform;
        }

        if (hpPlayerParent == null)
        {
            Debug.LogError("PlayerHealthUI: HPPlayer not found!");
            return;
        }

        healthSegments.Clear();
        for (int i = 0; i < hpPlayerParent.childCount; i++)
        {
            GameObject segment = hpPlayerParent.GetChild(i).gameObject;
            healthSegments.Add(segment);
            Debug.Log($"PlayerHealthUI: Segment {i} = {segment.name}");
        }

        Debug.Log($"PlayerHealthUI: Found {healthSegments.Count} health segments");
    }

    private void Update()
    {
        int currentHealth;
        if (GameLoopManager.Instance != null)
        {
            currentHealth = GameLoopManager.Instance.currentHealth;
            testHealth = currentHealth;
            testMaxHealth = GameLoopManager.Instance.maxHealth;
        }
        else
        {
            currentHealth = testHealth;
        }

        if (currentHealth != lastHealth)
        {
            UpdateHealth(currentHealth);
        }

        if (Input.GetKeyDown(damageKey)) { TestDamage(); }
        if (Input.GetKeyDown(healKey)) { TestHeal(); }
        if (Input.GetKeyDown(resetKey)) { TestReset(); }
        if (Input.GetKeyDown(initKey)) { TestInitialize(); }
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
            if (i < segmentsToHide)
            {
                healthSegments[i].SetActive(false);
            }
            else
            {
                healthSegments[i].SetActive(true);
            }
        }

        lastHealth = currentHealth;
        
        Debug.Log($"PlayerHealthUI: Updated to {currentHealth}/{maxHealth} health. Hidden {segmentsToHide} segments from left.");
    }

    private void ShowAllSegments()
    {
        foreach (GameObject segment in healthSegments)
        {
            segment.SetActive(true);
        }
    }

    public void TestDamage()
    {
        Debug.Log("PlayerHealthUI: Q key pressed!");
        if (GameLoopManager.Instance != null)
        {
            int healthBefore = GameLoopManager.Instance.currentHealth;
            GameLoopManager.Instance.TakeDamage();
            int healthAfter = GameLoopManager.Instance.currentHealth;
            Debug.Log($"Test: Player took 1 damage. HP: {healthBefore} -> {healthAfter}/{GameLoopManager.Instance.maxHealth}");
        }
        else
        {
            testHealth--;
            if (testHealth < 0) testHealth = 0;
            UpdateHealth(testHealth);
            Debug.Log($"Test: Player took 1 damage. HP: {testHealth + 1} -> {testHealth}/{testMaxHealth}");
        }
    }

    public void TestHeal()
    {
        if (GameLoopManager.Instance != null)
        {
            int currentHealth = GameLoopManager.Instance.currentHealth;
            int maxHealth = GameLoopManager.Instance.maxHealth;
            if (currentHealth < maxHealth)
            {
                GameLoopManager.Instance.Heal();
                Debug.Log($"Test: Player healed. Current HP: {GameLoopManager.Instance.currentHealth}/{maxHealth}");
            }
            else
            {
                Debug.Log("Test: Player already at full health!");
            }
        }
        else
        {
            if (testHealth < testMaxHealth)
            {
                testHealth++;
                UpdateHealth(testHealth);
                Debug.Log($"Test: Player healed. Current HP: {testHealth}/{testMaxHealth}");
            }
            else
            {
                Debug.Log("Test: Player already at full health!");
            }
        }
    }

    public void TestReset()
    {
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.currentHealth = GameLoopManager.Instance.maxHealth;
            UpdateHealth(GameLoopManager.Instance.currentHealth);
            Debug.Log($"Test: Player reset to full health. HP: {GameLoopManager.Instance.currentHealth}/{GameLoopManager.Instance.maxHealth}");
        }
        else
        {
            testHealth = testMaxHealth;
            UpdateHealth(testHealth);
            Debug.Log($"Test: Player reset to full health. HP: {testHealth}/{testMaxHealth}");
        }
    }

    public void TestInitialize()
    {
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.currentHealth = GameLoopManager.Instance.maxHealth;
            UpdateHealth(GameLoopManager.Instance.currentHealth);
            Debug.Log($"Test: Player initialized. HP: {GameLoopManager.Instance.currentHealth}/{GameLoopManager.Instance.maxHealth}");
        }
        else
        {
            testHealth = testMaxHealth;
            UpdateHealth(testHealth);
            Debug.Log($"Test: Player initialized. HP: {testHealth}/{testMaxHealth}");
        }
    }
}

