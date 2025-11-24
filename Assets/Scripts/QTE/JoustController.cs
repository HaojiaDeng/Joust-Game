using UnityEngine;
using System.Collections;

public class JoustController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform enemy;
    [SerializeField] private Camera mainCamera;
    
    private CardData currentCard;
    
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
            // Play success animation
            Debug.Log("Perfect QTE!");
        }
        else
        {
            // Play failure animation
            Debug.Log("Failed or partial QTE");
        }
        
        // Apply damage, update game state
    }
}