using UnityEngine;
using UnityEngine.UI;

public class BossHealthTester : MonoBehaviour
{
    [Header("Keyboard Test Controls")]
    [Tooltip("Press this key to damage Boss (default: D)")]
    public KeyCode damageKey = KeyCode.D;
    
    [Tooltip("Press this key to heal Boss (default: H)")]
    public KeyCode healKey = KeyCode.H;
    
    [Tooltip("Press this key to reset Boss health (default: R)")]
    public KeyCode resetKey = KeyCode.R;
    
    [Tooltip("Press this key to initialize Boss (default: I)")]
    public KeyCode initKey = KeyCode.I;

    [Header("UI Buttons (Optional)")]
    public Button damageButton;
    public Button healButton;
    public Button resetButton;
    public Button initializeButton;

    private void Start()
    {
        Debug.Log("BossHealthTester: Script started! Ready for testing. Press I to initialize, D to damage, H to heal, R to reset.");
        
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(TestDamage);
        }

        if (healButton != null)
        {
            healButton.onClick.AddListener(TestHeal);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(TestReset);
        }

        if (initializeButton != null)
        {
            initializeButton.onClick.AddListener(TestInitialize);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(damageKey))
        {
            Debug.Log("BossHealthTester: D key pressed!");
            TestDamage();
        }

        if (Input.GetKeyDown(healKey))
        {
            Debug.Log("BossHealthTester: H key pressed!");
            TestHeal();
        }

        if (Input.GetKeyDown(resetKey))
        {
            Debug.Log("BossHealthTester: R key pressed!");
            TestReset();
        }

        if (Input.GetKeyDown(initKey))
        {
            Debug.Log("BossHealthTester: I key pressed!");
            TestInitialize();
        }
    }

    public void TestDamage()
    {
        Debug.Log("TestDamage: D key pressed!");
        
        if (BossStats.Instance == null)
        {
            Debug.LogError("Test: BossStats.Instance is null! Make sure BossSystem GameObject exists in scene.");
            return;
        }

        if (!BossStats.Instance.IsActive())
        {
            Debug.Log("Test: Boss not active, initializing first...");
            BossStats.Instance.InitializeBoss();
        }

        int healthBefore = BossStats.Instance.GetCurrentHealth();
        BossStats.Instance.TakeDamage(1);
        int healthAfter = BossStats.Instance.GetCurrentHealth();
        
        Debug.Log($"Test: Boss took 1 damage. HP: {healthBefore} -> {healthAfter}/{BossStats.Instance.GetMaxHealth()}");
        
        if (BossHealthUI.Instance != null)
        {
            Debug.Log($"Test: BossHealthUI.Instance exists, should update UI");
        }
        else
        {
            Debug.LogWarning("Test: BossHealthUI.Instance is null!");
        }
    }

    public void TestHeal()
    {
        if (BossStats.Instance != null && BossStats.Instance.IsActive())
        {
            int currentHealth = BossStats.Instance.GetCurrentHealth();
            int maxHealth = BossStats.Instance.GetMaxHealth();
            if (currentHealth < maxHealth)
            {
                BossStats.Instance.Heal(1);
                Debug.Log($"Test: Boss healed. Current HP: {BossStats.Instance.GetCurrentHealth()}/{maxHealth}");
            }
            else
            {
                Debug.Log("Test: Boss already at full health!");
            }
        }
        else
        {
            Debug.LogWarning("Test: Boss not initialized!");
        }
    }

    public void TestReset()
    {
        if (BossStats.Instance != null)
        {
            BossStats.Instance.InitializeBoss();
            Debug.Log($"Test: Boss reset to full health. HP: {BossStats.Instance.GetCurrentHealth()}/{BossStats.Instance.GetMaxHealth()}");
        }
        else
        {
            Debug.LogWarning("Test: BossStats.Instance is null!");
        }
    }

    public void TestInitialize()
    {
        if (BossStats.Instance != null)
        {
            BossStats.Instance.InitializeBoss();
            Debug.Log($"Test: Boss initialized. HP: {BossStats.Instance.GetCurrentHealth()}/{BossStats.Instance.GetMaxHealth()}");
        }
        else
        {
            Debug.LogWarning("Test: BossStats.Instance is null!");
        }
    }
}

