using UnityEngine;
using System.Collections;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private QTEUIController uiController;
    private QTEResult lastQTEResult;
    
    private void Awake()
    {
        TryFindUIController("Awake");
    }

    private void TryFindUIController(string context)
    {
        if (uiController != null) return;

        uiController = GetComponentInChildren<QTEUIController>(true);
        if (uiController == null)
        {
            uiController = GetComponent<QTEUIController>();
        }
        if (uiController == null)
        {
            GameObject promptPanel = GameObject.Find("PromptPanel");
            if (promptPanel != null)
            {
                uiController = promptPanel.GetComponent<QTEUIController>();
                if (uiController == null)
                {
                    uiController = promptPanel.GetComponentInChildren<QTEUIController>(true);
                }
            }
        }
        if (uiController == null)
        {
            QTEUIController[] allControllers = Resources.FindObjectsOfTypeAll<QTEUIController>();
            Transform rootTransform = transform.root;
            foreach (var controller in allControllers)
            {
                if (controller != null && controller.gameObject.scene.isLoaded)
                {
                    bool isInSameRoot = controller.transform.root == rootTransform;
                    bool isChild = controller.transform.IsChildOf(transform);
                    if (isChild || isInSameRoot)
                    {
                        uiController = controller;
                        Debug.Log($"Found QTEUIController: {controller.name} (in {context})");
                        break;
                    }
                }
            }
        }
        
        if (uiController != null)
        {
            Debug.Log($"QTEManager: Auto-found QTEUIController in {context}: {uiController.name} on {uiController.gameObject.name}");
        }
    }

    private void OnEnable()
    {
        TryFindUIController("OnEnable");
    }
    
    public IEnumerator ExecuteQTE(QTEPattern pattern)
    {
        Debug.Log("ExecuteQTE called!");
        
        TryFindUIController("ExecuteQTE");
        
        QTEResult result = new QTEResult();
        
        if (uiController == null)
        {
            Debug.LogError("QTEUIController is not assigned to QTEManager and could not be found!");
            Debug.LogError($"QTEManager GameObject: {gameObject.name}, Active: {gameObject.activeInHierarchy}, Scene: {gameObject.scene.name}");
            result.totalInputs = 1;
            result.successfulInputs = 0;
            result.damageMultiplier = 0f;
            lastQTEResult = result;
            yield break;
        }
        
        Debug.Log($"UI Controller found: {uiController.name}");
        Debug.Log($"QTE Pattern: {pattern.patternName}, Type: {pattern.qteType}");
        
        BaseQTE qteExecutor = GetQTEExecutor(pattern.qteType);
        Debug.Log($"QTE Executor created: {qteExecutor.GetType().Name}");
        
        yield return qteExecutor.Execute(pattern, result, uiController);
        
        Debug.Log($"QTE execution complete. Result: {result.successfulInputs}/{result.totalInputs}");
        
        lastQTEResult = result;
    }
    
    public QTEResult GetLastQTEResult()
    {
        return lastQTEResult;
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