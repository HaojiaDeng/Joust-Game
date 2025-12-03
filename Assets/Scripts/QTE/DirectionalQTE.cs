using UnityEngine;
using System.Collections;

public class DirectionalQTE : BaseQTE
{
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        result.totalInputs = pattern.inputSequence.Count;
        result.successfulInputs = 0;
        
        foreach (QTEInput input in pattern.inputSequence)
        {
            yield return new WaitForSeconds(input.delayBeforeThisPrompt);
            
            ui.ShowPrompt(input.requiredKey, input.promptIcon);
            
            yield return StartCoroutine(WaitForDirectionalInput(input.requiredKey, input.windowDuration, result));
            
            if (result.lastInputSuccess)
            {
                result.successfulInputs++;
                ui.ShowSuccess();
                yield return new WaitForSeconds(0.2f); // Brief delay to show success
            }
            else
            {
                ui.ShowFailure();
                // Visual feedback for failure
                if (GameLoopManager.Instance != null) 
                {
                    GameLoopManager.Instance.TakeDamage();
                    GameLoopManager.Instance.ShowCenterMessage("MISS!", 0.5f);
                }
            }
            
            yield return new WaitForSeconds(pattern.gapBetweenInputs);
        }
        
        CalculateFinalResult(pattern, result);
    }
    
    private IEnumerator WaitForDirectionalInput(KeyCode expectedKey, float maxTime, QTEResult result)
    {
        float elapsed = 0f;
        
        // Map of arrow keys for directional input
        KeyCode[] directionalKeys = { 
            KeyCode.UpArrow, 
            KeyCode.DownArrow, 
            KeyCode.LeftArrow, 
            KeyCode.RightArrow 
        };
        
        while (elapsed < maxTime)
        {
            // Check if correct directional key pressed
            if (CheckInput(expectedKey))
            {
                result.lastInputSuccess = true;
                yield break;
            }
            
            // Penalize wrong directional inputs (optional)
            foreach (KeyCode key in directionalKeys)
            {
                if (key != expectedKey && CheckInput(key))
                {
                    result.lastInputSuccess = false;
                    yield break;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        result.lastInputSuccess = false;
    }
}