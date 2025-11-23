using UnityEngine;
using System.Collections;

public class HoldAndReleaseQTE : BaseQTE
{
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        result.totalInputs = pattern.inputSequence.Count;
        result.successfulInputs = 0;
        
        foreach (QTEInput input in pattern.inputSequence)
        {
            yield return new WaitForSeconds(input.delayBeforeThisPrompt);
            
            // Phase 1: Wait for hold
            ui.ShowPrompt(input.requiredKey, input.promptIcon);
            
            bool isHolding = false;
            float holdStartTime = 0f;
            float elapsed = 0f;
            
            // Wait for initial press
            while (elapsed < input.windowDuration && !isHolding)
            {
                if (Input.GetKeyDown(input.requiredKey))
                {
                    isHolding = true;
                    holdStartTime = Time.time;
                    ui.ShowSuccess(); // Feedback that hold started
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!isHolding)
            {
                ui.ShowFailure();
                yield return new WaitForSeconds(pattern.gapBetweenInputs);
                continue;
            }
            
            // Phase 2: Wait for release at optimal time
            float optimalHoldTime = pattern.gapBetweenInputs; // How long to hold
            float releaseWindow = 0.2f; // Window around optimal time
            
            bool released = false;
            bool releasedInWindow = false;
            
            while (!released)
            {
                if (Input.GetKeyUp(input.requiredKey))
                {
                    released = true;
                    float holdDuration = Time.time - holdStartTime;
                    
                    // Check if released in optimal window
                    if (Mathf.Abs(holdDuration - optimalHoldTime) <= releaseWindow)
                    {
                        releasedInWindow = true;
                        result.successfulInputs++;
                        ui.ShowSuccess();
                    }
                    else
                    {
                        ui.ShowFailure();
                    }
                }
                
                yield return null;
            }
            
            yield return new WaitForSeconds(pattern.gapBetweenInputs);
        }
        
        CalculateFinalResult(pattern, result);
    }
}