using UnityEngine;
using System.Collections;

public abstract class BaseQTE : MonoBehaviour
{
    public abstract IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui);

    protected bool CheckInput(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    protected void CalculateFinalResult(QTEPattern pattern, QTEResult result)
    {
        result.accuracyPercent = (float)result.successfulInputs / result.totalInputs * 100f;
        result.isPerfect = result.successfulInputs == result.totalInputs;

        if (result.isPerfect)
            result.damageMultiplier = pattern.perfectBonus;
        else if (result.successfulInputs >= pattern.minimumSuccessfulInputs)
            result.damageMultiplier = 1f;
        else
            result.damageMultiplier = pattern.failurePenalty;
    }
}