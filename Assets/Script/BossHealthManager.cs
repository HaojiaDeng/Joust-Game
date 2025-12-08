using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BossHealthManager : MonoBehaviour
{
    public static BossHealthManager Instance;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;
    
    [Header("Grid Health UI")]
    [SerializeField] private Transform healthGridParent; // The parent containing all health squares
    [SerializeField] private Color activeHealthColor = Color.red;
    [SerializeField] private Color depletedHealthColor = Color.gray;
    
    private List<Image> healthSquares = new List<Image>();
    
    [Header("Optional Text Display")]
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;
    
    [Header("Visual Feedback")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private SpriteRenderer bossSprite;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Start()
    {
        InitializeHealthGrid();
        currentHealth = maxHealth;
        UpdateHealthUI();
        
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }
    
    private void InitializeHealthGrid()
    {
        // Get all Image components from children of the health grid parent
        if (healthGridParent != null)
        {
            healthSquares.Clear();
            
            foreach (Transform child in healthGridParent)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    healthSquares.Add(img);
                }
            }
            
            // Set max health to match number of squares
            maxHealth = healthSquares.Count;
            
            Debug.Log($"Initialized {healthSquares.Count} health squares");
        }
        else
        {
            Debug.LogError("Health Grid Parent is not assigned!");
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"Boss took {damage} damage! Current health: {currentHealth}/{maxHealth}");
        
        UpdateHealthUI();
        PlayDamageEffect();
        
        if (currentHealth <= 0)
        {
            OnBossDefeated();
        }
    }
    
    private void UpdateHealthUI()
    {
        // Update grid squares
        for (int i = 0; i < healthSquares.Count; i++)
        {
            if (i < currentHealth)
            {
                // This square should be active (red)
                healthSquares[i].color = activeHealthColor;
            }
            else
            {
                // This square should be depleted (gray)
                healthSquares[i].color = depletedHealthColor;
            }
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
    
    private void PlayDamageEffect()
    {
        // Play damage animation
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger("Hit");
        }
        
        // Flash sprite red
        if (bossSprite != null)
        {
            StartCoroutine(FlashSprite());
        }
    }
    
    private System.Collections.IEnumerator FlashSprite()
    {
        Color originalColor = bossSprite.color;
        bossSprite.color = damageFlashColor;
        yield return new WaitForSeconds(0.1f);
        bossSprite.color = originalColor;
    }
    
    private void OnBossDefeated()
    {
        Debug.Log("Boss defeated!");
        
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger("Death");
        }
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }
    
    // Getters for boss battle data
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
}