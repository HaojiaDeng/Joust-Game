using UnityEngine;

public class BossStats : MonoBehaviour
{
    public static BossStats Instance;

    private const int BOSS_MAX_HEALTH = 10;
    
    public int currentHealth; // Made public for visual feedback damage system
    private bool isActive = false;
    private CardStats.CardData bossCurrentCard;

    private void Awake()
    {
        Instance = this;
        currentHealth = BOSS_MAX_HEALTH;
    }

    public void InitializeBoss()
    {
        currentHealth = BOSS_MAX_HEALTH;
        isActive = true;
        bossCurrentCard = null;

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.UpdateHealth(currentHealth);
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return BOSS_MAX_HEALTH;
    }

    public void TakeDamage(int damage)
    {
        if (!isActive) return;

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log($"Boss took {damage} damage. Health: {currentHealth}/{BOSS_MAX_HEALTH}");

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.UpdateHealth(currentHealth);
        }
    }

    public void Heal(int amount)
    {
        if (!isActive) return;

        currentHealth += amount;
        if (currentHealth > BOSS_MAX_HEALTH)
        {
            currentHealth = BOSS_MAX_HEALTH;
        }

        Debug.Log($"Boss healed {amount}. Health: {currentHealth}/{BOSS_MAX_HEALTH}");

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.UpdateHealth(currentHealth);
        }
    }

    public bool IsDefeated()
    {
        return isActive && currentHealth <= 0;
    }

    public void SetCurrentCard(CardStats.CardData card)
    {
        bossCurrentCard = card;
    }

    public CardStats.CardData GetCurrentCard()
    {
        return bossCurrentCard;
    }

    public void DefeatBoss()
    {
        isActive = false;
        Debug.Log("Boss defeated!");
    }
}

