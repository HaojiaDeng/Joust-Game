using UnityEngine;
using System.Collections;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private QTEUIController uiController;
    private QTEResult lastQTEResult;
    
    public IEnumerator ExecuteQTE(QTEPattern pattern)
    {
        
        // Check if UI controller is assigned
        if (uiController == null)
        {
            Debug.LogError("QTEUIController is not assigned to QTEManager!");
            yield break;
        }
        
        
        BaseQTE qteExecutor = GetQTEExecutor(pattern.qteType);
        
        QTEResult result = new QTEResult();
        
        yield return qteExecutor.Execute(pattern, result, uiController);
        
        
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
        
        switch (type)
        {
            case QTEType.Sequence:
                return gameObject.AddComponent<SequenceQTE>();
            case QTEType.ComboInput:
                return gameObject.AddComponent<ComboInputQTE>();
            case QTEType.ButtonMash:
                return gameObject.AddComponent<ButtonMashQTE>();
            case QTEType.HoldAndRelease:
                return gameObject.AddComponent<HoldAndReleaseQTE>();
            default:
                return gameObject.AddComponent<SequenceQTE>();
        }
    }
}