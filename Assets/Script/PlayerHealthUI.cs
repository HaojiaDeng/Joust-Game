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
        if (Instance == null)
        {
            Instance = this;
            Debug.Log($"PlayerHealthUI: Awake called, Instance set successfully on {gameObject.name}");
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"PlayerHealthUI: Multiple instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        Debug.Log("PlayerHealthUI: Start called");
        
        // Initialize health segments in Start() to ensure all GameObjects are ready
        InitializeHealthSegments();
        
        if (GameLoopManager.Instance != null)
        {
            testHealth = GameLoopManager.Instance.currentHealth;
            testMaxHealth = GameLoopManager.Instance.maxHealth;
            // Update display with initial health
            UpdateHealth(GameLoopManager.Instance.currentHealth);
        }
        Debug.Log("PlayerHealthUI: Script started! Ready for testing. Press P to initialize, Q to damage, E to heal, T to reset.");
    }

    private void InitializeHealthSegments()
    {
        Debug.Log("PlayerHealthUI: InitializeHealthSegments called");
        
        if (hpPlayerParent == null)
        {
            Debug.Log("PlayerHealthUI: hpPlayerParent is null, searching for HPPlayer GameObject...");
            hpPlayerParent = GameObject.Find("HPPlayer")?.transform;
            
            if (hpPlayerParent == null)
            {
                // Try searching in Canvas
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    Transform found = canvas.transform.Find("HPPlayer");
                    if (found == null)
                    {
                        // Try recursive search
                        found = FindInChildren(canvas.transform, "HPPlayer");
                    }
                    hpPlayerParent = found;
                }
            }
        }

        if (hpPlayerParent == null)
        {
            Debug.LogError("PlayerHealthUI: HPPlayer not found! Make sure there's a GameObject named 'HPPlayer' in the scene.");
            return;
        }

        Debug.Log($"PlayerHealthUI: Found HPPlayer at {hpPlayerParent.gameObject.name}, path: {GetGameObjectPath(hpPlayerParent.gameObject)}");

        healthSegments.Clear();
        int childCount = hpPlayerParent.childCount;
        Debug.Log($"PlayerHealthUI: HPPlayer has {childCount} children");
        
        for (int i = 0; i < childCount; i++)
        {
            Transform child = hpPlayerParent.GetChild(i);
            if (child != null)
            {
                GameObject segment = child.gameObject;
                healthSegments.Add(segment);
                Debug.Log($"PlayerHealthUI: Segment {i} = {segment.name}, Active: {segment.activeSelf}, Position: {segment.transform.position}");
            }
        }

        Debug.Log($"PlayerHealthUI: Found {healthSegments.Count} health segments. Expected: 10");
        
        if (healthSegments.Count != 10)
        {
            Debug.LogWarning($"PlayerHealthUI: WARNING! Expected 10 health segments but found {healthSegments.Count}. Player health display may be incorrect.");
        }
    }
    
    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform found = FindInChildren(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
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
        Debug.Log($"PlayerHealthUI.UpdateHealth called with currentHealth = {currentHealth}");
        
        if (healthSegments.Count == 0)
        {
            Debug.Log("PlayerHealthUI: healthSegments is empty, initializing...");
            InitializeHealthSegments();
        }

        if (healthSegments.Count == 0)
        {
            Debug.LogError("PlayerHealthUI: No health segments found! Cannot update health display.");
            return;
        }

        // Use GameLoopManager's maxHealth (should be 10) instead of segment count
        int maxHealth = GameLoopManager.Instance != null ? GameLoopManager.Instance.maxHealth : healthSegments.Count;
        int segmentsToHide = maxHealth - currentHealth;
        
        // Clamp to valid range
        segmentsToHide = Mathf.Clamp(segmentsToHide, 0, healthSegments.Count);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"PlayerHealthUI: Updating display - Current: {currentHealth}, Max: {maxHealth}, Segments: {healthSegments.Count}, ToHide: {segmentsToHide}");
        
        // Show all segments first
        for (int i = 0; i < healthSegments.Count; i++)
        {
            if (healthSegments[i] != null)
            {
                healthSegments[i].SetActive(true);
            }
            else
            {
                Debug.LogError($"PlayerHealthUI: Segment {i} is NULL!");
            }
        }
        
        // Then hide segments from left (representing lost health)
        int hiddenCount = 0;
        for (int i = 0; i < segmentsToHide && i < healthSegments.Count; i++)
        {
            if (healthSegments[i] != null)
            {
                healthSegments[i].SetActive(false);
                hiddenCount++;
                Debug.Log($"PlayerHealthUI: Hiding segment {i} ({healthSegments[i].name})");
            }
            else
            {
                Debug.LogError($"PlayerHealthUI: Cannot hide segment {i} - it's NULL!");
            }
        }

        lastHealth = currentHealth;
        
        Debug.Log($"PlayerHealthUI: Updated to {currentHealth}/{maxHealth} health. Hidden {hiddenCount} segments. {healthSegments.Count - hiddenCount}/{healthSegments.Count} segments visible.");
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

