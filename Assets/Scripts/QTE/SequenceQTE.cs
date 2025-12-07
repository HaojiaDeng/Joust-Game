using UnityEngine;
using System.Collections;

public class SequenceQTE : BaseQTE
{
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        Debug.Log("SequenceQTE Execute started!");
        Debug.Log($"Input sequence count: {pattern.inputSequence.Count}");
        
        result.totalInputs = pattern.inputSequence.Count;
        result.successfulInputs = 0;
        
        foreach (QTEInput input in pattern.inputSequence)
        {
            Debug.Log($"Processing input: {input.requiredKey}, Window: {input.windowDuration}");
            
            // Wait for delay before this prompt
            yield return new WaitForSeconds(input.delayBeforeThisPrompt);
            
            Debug.Log("About to show prompt...");
            
            // Show prompt to player
            ui.ShowPrompt(input.requiredKey, input.promptIcon);
            
            Debug.Log("Prompt shown, waiting for input...");
            
            // Wait for input
            yield return StartCoroutine(WaitForInput(input.requiredKey, input.windowDuration, result));
            
            Debug.Log($"Input result: {result.lastInputSuccess}");
            
            if (result.lastInputSuccess)
            {
                result.successfulInputs++;
                ui.ShowSuccess();
                yield return new WaitForSeconds(0.2f); // Brief delay to show success
            }
            else
            {
                // Failed once - QTE fails immediately
                ui.ShowFailure();
                if (GameLoopManager.Instance != null)
                {
                    GameLoopManager.Instance.ShowCenterMessage("MISS! QTE FAILED!", 1f);
                }
                
                // Mark QTE as failed and exit immediately
                result.successfulInputs = 0; // Set to 0 to indicate complete failure
                result.isPerfect = false;
                result.damageMultiplier = 0f;
                result.accuracyPercent = 0f;
                
                Debug.Log("QTE FAILED - Single failure triggers immediate failure!");
                yield break;
            }
            
            // Gap before next input
            yield return new WaitForSeconds(pattern.gapBetweenInputs);
        }
        
        Debug.Log("All inputs processed, calculating result...");
        CalculateFinalResult(pattern, result);
    }
    
    private IEnumerator WaitForInput(KeyCode key, float maxTime, QTEResult result)
    {
        float elapsed = 0f;
        
        while (elapsed < maxTime)
        {
            if (CheckInput(key))
            {
                result.lastInputSuccess = true;
                yield break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        result.lastInputSuccess = false;
    }
}