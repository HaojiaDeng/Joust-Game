using UnityEngine;
using System.Collections;

public class SequenceQTE : BaseQTE
{
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        result.totalInputs = pattern.inputSequence.Count;
        result.successfulInputs = 0;

        foreach (QTEInput input in pattern.inputSequence)
        {
            // Wait for delay before this prompt
            yield return new WaitForSeconds(input.delayBeforeThisPrompt);

            // Show prompt to player
            ui.ShowPrompt(input.requiredKey, input.promptIcon);

            // Wait for input
            yield return StartCoroutine(WaitForInput(input.requiredKey, input.windowDuration, result));

            if (result.GetLastQTEResult())
            {
                result.successfulInputs++;
                ui.ShowSuccess();
            }
            else
            {
                ui.ShowFailure();
            }

            // Gap before next input
            yield return new WaitForSeconds(pattern.gapBetweenInputs);
        }

        CalculateFinalResult(pattern, result);
    }

    private IEnumerator WaitForInput(KeyCode key, float maxTime, QTEResult result)
    {
        float elapsed = 0f;

        while (elapsed < maxTime)
        {
            if (CheckInput(key))
            {
                result.InputSuccess(true);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        result.InputSuccess(false);
    }
}