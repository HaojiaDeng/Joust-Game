using UnityEngine;
using System.Collections;

public class ButtonMashQTE : BaseQTE
{
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        // For button mash, we use the first input in the sequence
        QTEInput mashInput = pattern.inputSequence[0];
        KeyCode mashKey = mashInput.requiredKey;
        float duration = mashInput.windowDuration;
        
        int targetPresses = pattern.minimumSuccessfulInputs;
        int actualPresses = 0;
        
        result.totalInputs = targetPresses;
        
        // Show the prompt
        ui.ShowPrompt(mashKey, mashInput.promptIcon);
        
        float elapsed = 0f;
        bool lastFramePressed = false;
        
        while (elapsed < duration)
        {
            // Detect key press (prevent holding)
            bool currentlyPressed = Input.GetKey(mashKey);
            
            if (currentlyPressed && !lastFramePressed)
            {
                actualPresses++;
                ui.ShowSuccess(); // Visual feedback per press
            }
            
            lastFramePressed = currentlyPressed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        ui.HidePrompt();
        
        result.successfulInputs = Mathf.Min(actualPresses, targetPresses);
        CalculateFinalResult(pattern, result);
    }
}