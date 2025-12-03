using UnityEngine;
using System.Collections;

public class RhythmQTE : BaseQTE
{
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        result.totalInputs = pattern.inputSequence.Count;
        result.successfulInputs = 0;
        
        float beatInterval = pattern.gapBetweenInputs;
        
        foreach (QTEInput input in pattern.inputSequence)
        {
            // Show prompt slightly before the beat
            float anticipationTime = 0.3f;
            yield return new WaitForSeconds(input.delayBeforeThisPrompt);
            
            ui.ShowPrompt(input.requiredKey, input.promptIcon);
            yield return new WaitForSeconds(anticipationTime);
            
            // The "perfect" beat moment
            float perfectWindow = 0.1f; // Tight window for perfect timing
            float okayWindow = input.windowDuration; // Wider window for okay timing
            
            bool hitPerfect = false;
            bool hitOkay = false;
            float elapsed = 0f;
            
            // Check for perfect timing
            while (elapsed < perfectWindow)
            {
                if (CheckInput(input.requiredKey))
                {
                    hitPerfect = true;
                    result.successfulInputs++;
                    ui.ShowSuccess();
                    yield return new WaitForSeconds(0.15f); // Brief delay to show success
                    break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // If not perfect, check for okay timing
            if (!hitPerfect)
            {
                while (elapsed < okayWindow)
                {
                    if (CheckInput(input.requiredKey))
                    {
                        hitOkay = true;
                        result.successfulInputs++;
                        ui.ShowSuccess();
                        break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            
            if (!hitPerfect && !hitOkay)
            {
                ui.ShowFailure();
                // Deduct health
                if (GameLoopManager.Instance != null)
                    GameLoopManager.Instance.TakeDamage();
            }
            
            // Wait for next beat
            yield return new WaitForSeconds(beatInterval - elapsed);
        }
        
        CalculateFinalResult(pattern, result);
    }
}