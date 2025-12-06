using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BattleRoundData
{
    public string weaponUsed;
    public WeaponCategory weaponCategory;
    public WeaponCategory enemyWeaponCategory;
    public int damageDealt;
    public float advantageMultiplier;
    public int qteSuccessfulInputs;
    public int qteTotalInputs;
    public float qteAccuracy;
    public bool wasPerfect;
}

public class BossBattleManager : MonoBehaviour
{
    public static BossBattleManager Instance;
    
    [Header("Battle Data")]
    private List<BattleRoundData> battleHistory = new List<BattleRoundData>();
    private int totalDamageDealt = 0;
    private int roundsPlayed = 0;
    private float battleStartTime;
    private bool battleActive = false;
    
    [Header("Current Round")]
    private BattleRoundData currentRound;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void StartBattle()
    {
        battleHistory.Clear();
        totalDamageDealt = 0;
        roundsPlayed = 0;
        battleStartTime = Time.time;
        battleActive = true;
        
        Debug.Log("Battle Started!");
    }
    
    public void StartRound(CardData playerCard, WeaponCategory enemyWeapon)
    {
        currentRound = new BattleRoundData
        {
            weaponUsed = playerCard.cardName,
            weaponCategory = playerCard.category,
            enemyWeaponCategory = enemyWeapon
        };
        
        // Calculate advantage
        if (WeaponAdvantageSystem.Instance != null)
        {
            currentRound.advantageMultiplier = WeaponAdvantageSystem.Instance.GetAdvantageMultiplier(
                playerCard.category,
                enemyWeapon
            );
        }
    }
    
    public void EndRound(QTEResult qteResult)
    {
        if (currentRound == null)
        {
            Debug.LogError("No current round to end!");
            return;
        }
        
        // Store QTE data
        currentRound.qteSuccessfulInputs = qteResult.successfulInputs;
        currentRound.qteTotalInputs = qteResult.totalInputs;
        currentRound.qteAccuracy = qteResult.accuracyPercent;
        currentRound.wasPerfect = qteResult.isPerfect;
        currentRound.damageDealt = qteResult.baseDamage;
        
        // Add to history
        battleHistory.Add(currentRound);
        totalDamageDealt += currentRound.damageDealt;
        roundsPlayed++;
        
        Debug.Log($"Round {roundsPlayed} complete: {currentRound.weaponUsed} dealt {currentRound.damageDealt} damage");
        
        currentRound = null;
    }
    
    public void EndBattle(bool victory)
    {
        battleActive = false;
        float battleDuration = Time.time - battleStartTime;
        
        Debug.Log("=== BATTLE SUMMARY ===");
        Debug.Log($"Result: {(victory ? "VICTORY" : "DEFEAT")}");
        Debug.Log($"Duration: {battleDuration:F1} seconds");
        Debug.Log($"Rounds Played: {roundsPlayed}");
        Debug.Log($"Total Damage Dealt: {totalDamageDealt}");
        Debug.Log($"Average Damage Per Round: {(roundsPlayed > 0 ? (float)totalDamageDealt / roundsPlayed : 0):F1}");
        
        // Log each round
        for (int i = 0; i < battleHistory.Count; i++)
        {
            var round = battleHistory[i];
            Debug.Log($"Round {i + 1}: {round.weaponUsed} ({round.weaponCategory} vs {round.enemyWeaponCategory}) " +
                     $"- Damage: {round.damageDealt} (x{round.advantageMultiplier}) " +
                     $"- QTE: {round.qteSuccessfulInputs}/{round.qteTotalInputs} ({round.qteAccuracy:F0}%)");
        }
    }
    
    // Getters for battle data
    public List<BattleRoundData> GetBattleHistory() => new List<BattleRoundData>(battleHistory);
    public int GetTotalDamageDealt() => totalDamageDealt;
    public int GetRoundsPlayed() => roundsPlayed;
    public float GetAverageDamage() => roundsPlayed > 0 ? (float)totalDamageDealt / roundsPlayed : 0;
    public bool IsBattleActive() => battleActive;
}