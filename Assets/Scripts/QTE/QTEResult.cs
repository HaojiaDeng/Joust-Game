[System.Serializable]
public class QTEResult
{
    public int successfulInputs;
    public int totalInputs;
    public bool isPerfect;
    public float accuracyPercent;
    public float damageMultiplier;
    public bool lastInputSuccess;

    public bool OverallSuccess()
    {
        return successfulInputs >= totalInputs / 2;
    }
    public bool GetLastQTEResult()
    {
        return lastInputSuccess;
    }
    public void InputSuccess(bool wasSuccessful)
    {
        lastInputSuccess = wasSuccessful;
    }
}