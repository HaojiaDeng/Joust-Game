using UnityEngine;

public class QTEDiagnostics : MonoBehaviour
{
    [ContextMenu("Check QTE Structure")]
    public void CheckQTEStructure()
    {
        Debug.Log("=== QTE Structure Diagnostics ===");
        
        GameObject qteUI = GameObject.Find("QTE");
        if (qteUI == null)
        {
            Debug.LogError("QTE GameObject not found!");
            return;
        }
        
        Debug.Log($"QTE GameObject found: {qteUI.name}, Active: {qteUI.activeSelf}, ActiveInHierarchy: {qteUI.activeInHierarchy}");
        
        QTEManager qteManager = qteUI.GetComponentInChildren<QTEManager>(true);
        if (qteManager == null)
        {
            qteManager = FindFirstObjectByType<QTEManager>();
            if (qteManager != null)
            {
                Debug.LogWarning($"QTEManager found OUTSIDE QTE hierarchy: {qteManager.gameObject.name} at path: {GetGameObjectPath(qteManager.gameObject)}");
            }
            else
            {
                Debug.LogError("QTEManager not found ANYWHERE in scene!");
            }
        }
        else
        {
            Debug.Log($"QTEManager found: {qteManager.name}, GameObject: {qteManager.gameObject.name}, Path: {GetGameObjectPath(qteManager.gameObject)}");
        }
        
        QTEUIController uiController = qteUI.GetComponentInChildren<QTEUIController>(true);
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
            
            if (uiController == null)
            {
                uiController = FindFirstObjectByType<QTEUIController>();
                if (uiController != null)
                {
                    Debug.LogWarning($"QTEUIController found OUTSIDE QTE hierarchy: {uiController.gameObject.name} at path: {GetGameObjectPath(uiController.gameObject)}");
                }
                else
                {
                    Debug.LogError("QTEUIController not found ANYWHERE in scene!");
                }
            }
            else
            {
                Debug.Log($"QTEUIController found on PromptPanel: {uiController.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"QTEUIController found: {uiController.name}, GameObject: {uiController.gameObject.name}, Path: {GetGameObjectPath(uiController.gameObject)}, Active: {uiController.gameObject.activeSelf}");
        }
        
        if (qteManager != null && uiController != null)
        {
            Debug.Log($"QTEManager.uiController is {(qteManager.GetComponent<QTEManager>() != null ? "ASSIGNED" : "NULL")}");
        }
        
        JoustController joustController = qteUI.GetComponentInChildren<JoustController>(true);
        if (joustController == null)
        {
            joustController = FindFirstObjectByType<JoustController>();
            if (joustController != null)
            {
                Debug.LogWarning($"JoustController found OUTSIDE QTE hierarchy: {joustController.gameObject.name} at path: {GetGameObjectPath(joustController.gameObject)}");
            }
            else
            {
                Debug.LogError("JoustController not found ANYWHERE in scene!");
            }
        }
        else
        {
            Debug.Log($"JoustController found: {joustController.name}");
        }
        
        QTEHealthManager healthManager = qteUI.GetComponentInChildren<QTEHealthManager>(true);
        if (healthManager == null)
        {
            healthManager = FindFirstObjectByType<QTEHealthManager>();
            if (healthManager != null)
            {
                Debug.LogWarning($"QTEHealthManager found OUTSIDE QTE hierarchy: {healthManager.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("QTEHealthManager not found - will be created at runtime if needed");
            }
        }
        else
        {
            Debug.Log($"QTEHealthManager found: {healthManager.name}");
        }
        
        Camera qteCamera = qteUI.GetComponentInChildren<Camera>(true);
        if (qteCamera == null)
        {
            Debug.LogWarning("Camera not found in QTE hierarchy!");
        }
        else
        {
            Debug.Log($"QTE Camera found: {qteCamera.name}, Enabled: {qteCamera.enabled}");
        }
        
        Debug.Log("=== Diagnostics Complete ===");
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}

