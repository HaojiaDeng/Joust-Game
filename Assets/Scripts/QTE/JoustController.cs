using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class JoustController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform enemy;
    [SerializeField] private Camera mainCamera;
    
    private CardData currentCard;
    
    [SerializeField] private int qteRoundsPerCard = 4;
    [SerializeField] private string qteSceneName = "SampleScene";
    
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
        Debug.Log("StartJoust called!");
        currentCard = card;
        StartCoroutine(JoustSequence());
    }
    
    private IEnumerator JoustSequence()
    {
        Debug.Log("JoustSequence started!");
        
        // Play charge animations
        yield return StartCoroutine(ChargeSequence());
        
        Debug.Log("ChargeSequence complete, starting QTE...");
        
        // Start QTE
        QTEManager qteManager = GetComponent<QTEManager>();
        if (qteManager == null)
        {
            Debug.LogError("QTEManager not found!");
            yield break;
        }
        
        yield return StartCoroutine(qteManager.ExecuteQTE(currentCard.qtePattern));
        QTEResult result = qteManager.GetLastQTEResult();
        
        Debug.Log("QTE complete!");
        
        // Handle result
        HandleJoustOutcome(result);
    }
    
    private IEnumerator ChargeSequence()
    {
        Debug.Log("ChargeSequence: Starting charge animation");
        
        if (player == null || enemy == null)
        {
            Debug.LogError("Player or Enemy transform is null!");
            yield break;
        }
        
        // Use current positions (already reset by ResetPositions)
        Vector3 playerStart = player.position;
        Vector3 enemyStart = enemy.position;
        Vector3 meetPoint = (playerStart + enemyStart) / 2;
        
        Debug.Log($"Player start: {playerStart}, Enemy start: {enemyStart}, Meet point: {meetPoint}");
        
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
        
        Debug.Log("ChargeSequence: Animation complete");
    }
    
    private void HandleJoustOutcome(QTEResult result)
    {
        Debug.Log("HandleJoustOutcome called!");
        
        if (currentCard == null)
        {
            Debug.LogError("Current card is null!");
            return;
        }
        
        int finalDamage = Mathf.RoundToInt(currentCard.baseDamage * result.damageMultiplier);
        
        Debug.Log($"QTE Complete! Success: {result.successfulInputs}/{result.totalInputs}, " +
                  $"Accuracy: {result.accuracyPercent}%, Damage Multiplier: {result.damageMultiplier}x, " +
                  $"Final Damage: {finalDamage}");
        
        if (result.isPerfect)
        {
            Debug.Log("Perfect QTE!");
        }
        else
        {
            Debug.Log("Failed or partial QTE");
        }
        
        QTEManager qteManager = GetComponent<QTEManager>();
        if (qteManager != null)
        {
            qteManager.ClearUI();
        }
        
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.Heal();
            
            var encounterManager = FindFirstObjectByType<RandomEncounterManager>();
            if (encounterManager != null)
            {
                encounterManager.OnQTECompleted();
            }

            if (!GameLoopManager.Instance.isGameOver)
            {
                if (GameLoopManager.Instance.roundsCompleted < qteRoundsPerCard)
                {
                    StartCoroutine(RestartLoop());
                }
                else
                {
                    Debug.Log($"QTE rounds ({GameLoopManager.Instance.roundsCompleted}) reached limit ({qteRoundsPerCard}). Will return to card phase after delay.");
                    StartCoroutine(EndQTEAndReturn());
                }
            }
        }
    }

    private IEnumerator EndQTEAndReturn()
    {
        yield return new WaitForSeconds(2f);

        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.roundsCompleted = 0;
        }

        if (CardManagement.Instance != null)
        {
            CardManagement.Instance.OnQTEFinished();
        }
        
        var qteScene = SceneManager.GetSceneByName(qteSceneName);
        if (qteScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(qteSceneName);
        }
    }

    private IEnumerator RestartLoop()
    {
        Debug.Log("RestartLoop: Waiting 2 seconds...");
        
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

        Debug.Log("RestartLoop: Finding RandomEncounterManager...");
        var manager = FindFirstObjectByType<RandomEncounterManager>();
        if (manager != null) 
        {
            Debug.Log("RestartLoop: Calling StartNewRound...");
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
            case QTEType.Directional:
                // Change hint after round 6 when new keys are introduced
                return currentRound < 6 ? "Press Arrow Keys!" : "Press the Keys!";
            case QTEType.ButtonMash:
                return "Mash the Key Fast!";
            case QTEType.Sequence:
                return "Press Keys in Order!";
            case QTEType.Rhythm:
                return "Press on the Beat!";
            case QTEType.HoldAndRelease:
                return "Hold & Release in Ring!";
            default:
                return "Get Ready!";
        }
    }
}