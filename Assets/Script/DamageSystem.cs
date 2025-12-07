using System.Collections;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    private static DamageSystem _instance;
    public static DamageSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject damageSystemObj = new GameObject("DamageSystem");
                _instance = damageSystemObj.AddComponent<DamageSystem>();
                DontDestroyOnLoad(damageSystemObj);
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void ApplyBossDamageWithVisualFeedback(int damage, string advantageText = "")
    {
        StartCoroutine(ApplyBossDamageCoroutine(damage, advantageText));
    }
    
    public void ApplyPlayerDamageWithVisualFeedback(int damage)
    {
        StartCoroutine(ApplyPlayerDamageCoroutine(damage));
    }
    
    private IEnumerator ApplyBossDamageCoroutine(int damage, string advantageText = "")
    {
        string damageMessage = $"Boss takes {damage} damage!";
        if (!string.IsNullOrEmpty(advantageText))
        {
            damageMessage += $" ({advantageText})";
        }
        
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.ShowCenterMessage(damageMessage, 2f);
        }
        
        for (int i = 0; i < damage; i++)
        {
            if (BossStats.Instance != null && BossStats.Instance.IsActive())
            {
                BossStats.Instance.currentHealth--;
                if (BossStats.Instance.currentHealth < 0)
                {
                    BossStats.Instance.currentHealth = 0;
                }
                
                if (BossHealthUI.Instance != null)
                {
                    BossHealthUI.Instance.UpdateHealth(BossStats.Instance.currentHealth);
                }
                
                if (BossStats.Instance.IsDefeated())
                {
                    BossStats.Instance.DefeatBoss();
                    if (GameLoopManager.Instance != null)
                    {
                        GameLoopManager.Instance.ShowCenterMessage("BOSS DEFEATED!", 3f);
                        GameLoopManager.Instance.isVictory = true;
                        Time.timeScale = 0f;
                    }
                    break;
                }
                
                if (i < damage - 1)
                {
                    yield return new WaitForSeconds(0.15f);
                }
            }
            else
            {
                break;
            }
        }
    }
    
    private IEnumerator ApplyPlayerDamageCoroutine(int damage)
    {
        for (int i = 0; i < damage; i++)
        {
            if (GameLoopManager.Instance != null && !GameLoopManager.Instance.isGameOver)
            {
                GameLoopManager.Instance.TakeDamage();
                
                if (i < damage - 1)
                {
                    yield return new WaitForSeconds(0.15f);
                }
            }
            else
            {
                break;
            }
        }
    }
    
    public static WeaponCategory ConvertCardTypeToWeaponCategory(CardStats.CardType cardType)
    {
        switch (cardType)
        {
            case CardStats.CardType.Sword:
            case CardStats.CardType.Axe:
                return WeaponCategory.SwordAxe;
            case CardStats.CardType.Bow:
            case CardStats.CardType.Lance:
                return WeaponCategory.BowLance;
            case CardStats.CardType.Shield:
                return WeaponCategory.Shield;
            default:
                return WeaponCategory.SwordAxe;
        }
    }
}
