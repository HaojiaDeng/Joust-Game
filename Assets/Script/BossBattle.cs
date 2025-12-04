using UnityEngine;
using System.Collections;

public class BossBattle : MonoBehaviour
{
    public static BossBattle Instance;

    private CardStats.CardData bossCard;
    private bool isBossBattleActive = false;

    private void Awake()
    {
        Instance = this;
    }

    public void StartBossBattle()
    {
        if (BossManager.Instance == null || BossStats.Instance == null)
        {
            Debug.LogError("BossManager or BossStats not found!");
            return;
        }

        if (!BossStats.Instance.IsActive())
        {
            BossManager.Instance.StartBossBattle();
        }

        isBossBattleActive = true;
        bossCard = BossManager.Instance.SelectBossCard();

        Debug.Log($"Boss Battle started! Boss: {bossCard.cardType}");
    }

    public CardStats.CardData GetBossCard()
    {
        return bossCard;
    }

    public bool IsBossBattleActive()
    {
        return isBossBattleActive;
    }

    public void CalculateBossDamage(float qteMultiplier)
    {
        if (!isBossBattleActive || bossCard == null)
        {
            return;
        }

        int baseDamage = 2;
        int finalDamage = Mathf.RoundToInt(baseDamage * qteMultiplier);

        if (BossStats.Instance != null)
        {
            BossStats.Instance.TakeDamage(finalDamage);
            
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.ShowCenterMessage($"Boss takes {finalDamage} damage!", 1.5f);
            }

            if (BossStats.Instance.IsDefeated())
            {
                BossStats.Instance.DefeatBoss();
                if (GameLoopManager.Instance != null)
                {
                    GameLoopManager.Instance.ShowCenterMessage("BOSS DEFEATED!", 3f);
                }
            }
        }

        Debug.Log($"Boss damage calculated: {finalDamage} (base:{baseDamage}, multiplier:{qteMultiplier})");
        Debug.Log($"Boss health: {BossStats.Instance.GetCurrentHealth()}/{BossStats.Instance.GetMaxHealth()}");

        ResetBattle();
    }

    private void ResetBattle()
    {
        bossCard = null;
        isBossBattleActive = false;
    }
}

