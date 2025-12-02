using UnityEngine;
using System.Collections;

public class SequenceQTE : BaseQTE
{
    private bool wasWrongKey = false;  // Track if failure was due to wrong key
    
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
            
            // Reset wrong key flag
            wasWrongKey = false;
            
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
                ui.ShowFailure();
                // Only deal damage for timeout (wrong key already dealt damage)
                if (!wasWrongKey && GameLoopManager.Instance != null) 
                {
                    GameLoopManager.Instance.TakeDamage();
                    GameLoopManager.Instance.ShowCenterMessage("MISS!", 0.5f);
                }
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
            // Check for correct key
            if (CheckInput(key))
            {
                result.lastInputSuccess = true;
                yield break;
            }
            
            // Check for wrong key press (any other game key)
            if (Input.anyKeyDown)
            {
                foreach (KeyCode wrongKey in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(wrongKey) && wrongKey != key && IsGameKey(wrongKey))
                    {
                        result.lastInputSuccess = false;
                        wasWrongKey = true;  // Mark that damage was dealt for wrong key
                        if (GameLoopManager.Instance != null)
                        {
                            GameLoopManager.Instance.TakeDamage();
                            GameLoopManager.Instance.ShowCenterMessage("WRONG KEY!", 0.5f);
                        }
                        yield break;
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        result.lastInputSuccess = false;
    }
    
    private bool IsGameKey(KeyCode key)
    {
        // Check if it's a relevant game key (not mouse buttons, modifiers, etc.)
        return (key >= KeyCode.A && key <= KeyCode.Z) ||
               (key >= KeyCode.UpArrow && key <= KeyCode.LeftArrow) ||
               key == KeyCode.Space;
    }
}