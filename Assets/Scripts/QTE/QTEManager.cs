using UnityEngine;
using System.Collections;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private QTEUIController uiController;
    private QTEResult lastQTEResult;
    
    public IEnumerator ExecuteQTE(QTEPattern pattern)
    {
        Debug.Log("ExecuteQTE called!");
        
        // Check if UI controller is assigned
        if (uiController == null)
        {
            Debug.LogError("QTEUIController is not assigned to QTEManager!");
            yield break;
        }
        
        Debug.Log($"UI Controller found: {uiController.name}");
        Debug.Log($"QTE Pattern: {pattern.patternName}, Type: {pattern.qteType}");
        
        BaseQTE qteExecutor = GetQTEExecutor(pattern.qteType);
        Debug.Log($"QTE Executor created: {qteExecutor.GetType().Name}");
        
        QTEResult result = new QTEResult();
        
        yield return qteExecutor.Execute(pattern, result, uiController);
        
        Debug.Log($"QTE execution complete. Result: {result.successfulInputs}/{result.totalInputs}");
        
        lastQTEResult = result;
    }
    
    public QTEResult GetLastQTEResult()
    {
        return lastQTEResult;
    }
    
    public void ClearUI()
    {
        if (uiController != null)
        {
            uiController.HidePrompt();
        }
    }
    
    private BaseQTE GetQTEExecutor(QTEType type)
    {
        Debug.Log($"GetQTEExecutor called with type: {type}");
        
        switch (type)
        {
            case QTEType.Sequence:
                return gameObject.AddComponent<SequenceQTE>();
            case QTEType.ButtonMash:
                return gameObject.AddComponent<ButtonMashQTE>();
            case QTEType.Directional:
                return gameObject.AddComponent<DirectionalQTE>();
            case QTEType.Rhythm:
                return gameObject.AddComponent<RhythmQTE>();
            case QTEType.HoldAndRelease:
                return gameObject.AddComponent<HoldAndReleaseQTE>();
            default:
                return gameObject.AddComponent<SequenceQTE>();
        }
    }
}