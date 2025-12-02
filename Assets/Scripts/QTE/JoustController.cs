using UnityEngine;
using System.Collections;

public class JoustController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform enemy;
    [SerializeField] private Camera mainCamera;
    
    private CardData currentCard;
    
    // Starting positions for reset
    private Vector3 playerStartPos = new Vector3(-5f, 0f, 0f);
    private Vector3 enemyStartPos = new Vector3(5f, 0f, 0f);
    
    public void ResetPositions()
    {
        if (player != null) player.position = playerStartPos;
        if (enemy != null) enemy.position = enemyStartPos;
    }
    
    public void StartJoust(CardData card)
    {
        currentCard = card;
        StartCoroutine(JoustSequence());
    }
    
    private IEnumerator JoustSequence()
    {
        
        // Play charge animations
        yield return StartCoroutine(ChargeSequence());
        
        
        // Start QTE
        QTEManager qteManager = GetComponent<QTEManager>();
        if (qteManager == null)
        {
            Debug.LogError("QTEManager not found!");
            yield break;
        }
        
        yield return StartCoroutine(qteManager.ExecuteQTE(currentCard.qtePattern));
        QTEResult result = qteManager.GetLastQTEResult();
        
        
        // Handle result
        HandleJoustOutcome(result);
    }
    
    private IEnumerator ChargeSequence()
    {
        
        if (player == null || enemy == null)
        {
            Debug.LogError("Player or Enemy transform is null!");
            yield break;
        }
        
        // Use current positions (already reset by ResetPositions)
        Vector3 playerStart = player.position;
        Vector3 enemyStart = enemy.position;
        Vector3 meetPoint = (playerStart + enemyStart) / 2;
        
        
        float duration = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            player.position = Vector3.Lerp(playerStart, meetPoint + Vector3.left * 0.5f, t);
            enemy.position = Vector3.Lerp(enemyStart, meetPoint + Vector3.right * 0.5f, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
    }
    
    private void HandleJoustOutcome(QTEResult result)
    {
        
        if (currentCard == null)
        {
            Debug.LogError("Current card is null!");
            return;
        }
        
        int finalDamage = Mathf.RoundToInt(currentCard.baseDamage * result.damageMultiplier);
        
        
        if (result.isPerfect)
        {
            // Perfect QTE
        }
        else
        {
            // Failed or partial QTE
        }
        
        // Clear QTE UI immediately after completion
        QTEManager qteManager = GetComponent<QTEManager>();
        if (qteManager != null)
        {
            qteManager.ClearUI();
        }
        
        if (GameLoopManager.Instance != null)
        {
            // Heal only when QTE was successful (damageMultiplier >= 1)
            if (result.damageMultiplier >= 1f)
            {
                GameLoopManager.Instance.Heal();
            }
            // Always advance to next round
            GameLoopManager.Instance.AdvanceRound();
            
            // Notify RandomEncounterManager of QTE completion
            var encounterManager = FindFirstObjectByType<RandomEncounterManager>();
            if (encounterManager != null)
            {
                encounterManager.OnQTECompleted();
            }
            
            if (!GameLoopManager.Instance.isGameOver)
            {
                StartCoroutine(RestartLoop());
            }
        }
    }

    private IEnumerator RestartLoop()
    {
        
        // Ensure UI is cleared at the start of restart
        QTEManager qteManager = GetComponent<QTEManager>();
        if (qteManager != null)
        {
            qteManager.ClearUI();
        }
        
        // Show round clear message
        if (GameLoopManager.Instance != null)
        {
            int round = GameLoopManager.Instance.roundsCompleted;
            GameLoopManager.Instance.ShowCenterMessage("ROUND CLEAR!\nGet Ready...", 2f);
        }

        yield return new WaitForSeconds(2f);

        // Reset positions
        ResetPositions();

        // Small delay before starting
        yield return new WaitForSeconds(0.5f);

        var manager = FindFirstObjectByType<RandomEncounterManager>();
        if (manager != null) 
        {
            manager.StartNewRound();
        }
        else
        {
            Debug.LogError("RestartLoop: RandomEncounterManager not found!");
        }
    }

    // Get hint text based on QTE type
    private string GetQTEHint(QTEType qteType)
    {
        int currentRound = GameLoopManager.Instance != null ? GameLoopManager.Instance.roundsCompleted : 0;
        
        switch (qteType)
        {
            case QTEType.Sequence:
                return currentRound < 4 ? "Press Arrow Keys One by One!" : "Press Keys One by One!";
            case QTEType.ComboInput:
                return currentRound < 4 ? "Input Full Arrow Combo!" : "Input Full Key Combo!";
            case QTEType.ButtonMash:
                return currentRound < 4 ? "Mash the Arrow Key Fast!" : "Mash the Key Fast!";
            case QTEType.HoldAndRelease:
                return currentRound < 4 ? "Hold & Release Arrow Key!" : "Hold & Release in Ring!";
            default:
                return "Get Ready!";
        }
    }
}