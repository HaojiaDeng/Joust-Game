using UnityEngine;
using System.Collections;

public class BossQTEListener : MonoBehaviour
{
    private QTEManager qteManager;
    private QTEResult lastResult;
    private bool hasCalculatedDamage = false;

    private void Start()
    {
        StartCoroutine(InitializeListener());
    }

    private IEnumerator InitializeListener()
    {
        Debug.Log("BossQTEListener: Starting initialization...");
        
        yield return new WaitForSeconds(1f);
        
        qteManager = FindFirstObjectByType<QTEManager>();
        
        Debug.Log($"BossQTEListener: QTEManager found: {(qteManager != null ? "yes" : "no")}");
        Debug.Log($"BossQTEListener: BossBattle.Instance: {(BossBattle.Instance != null ? "exists" : "null")}, IsActive: {(BossBattle.Instance != null ? BossBattle.Instance.IsBossBattleActive().ToString() : "N/A")}");
        
        if (BossBattle.Instance == null || !BossBattle.Instance.IsBossBattleActive())
        {
            Debug.LogWarning($"BossQTEListener: Boss battle not active. Instance: {(BossBattle.Instance != null ? "exists" : "null")}, Active: {(BossBattle.Instance != null ? BossBattle.Instance.IsBossBattleActive().ToString() : "N/A")}");
        }
        
        if (qteManager == null)
        {
            Debug.LogWarning("BossQTEListener: QTEManager not found, will keep trying...");
        }
        
        StartCoroutine(MonitorQTEComplete());
    }

    private IEnumerator MonitorQTEComplete()
    {
        Debug.Log("BossQTEListener: Starting to monitor QTE...");
        
        int checkCount = 0;
        bool qteStarted = false;
        
        while (!hasCalculatedDamage)
        {
            checkCount++;
            
            if (checkCount % 50 == 0)
            {
                Debug.Log($"BossQTEListener: Still monitoring... (check #{checkCount})");
            }
            
            if (qteManager == null)
            {
                qteManager = FindFirstObjectByType<QTEManager>();
                if (qteManager != null)
                {
                    Debug.Log("BossQTEListener: QTEManager found!");
                }
            }
            
            if (BossBattle.Instance == null || !BossBattle.Instance.IsBossBattleActive())
            {
                Debug.LogWarning("BossQTEListener: Boss battle no longer active, stopping monitor");
                break;
            }
            
            if (qteManager != null)
            {
                QTEResult result = qteManager.GetLastQTEResult();
                
                if (result != null && result.totalInputs > 0)
                {
                    if (!qteStarted)
                    {
                        qteStarted = true;
                        Debug.Log($"BossQTEListener: QTE detected! Total inputs: {result.totalInputs}");
                    }
                    
                    if (lastResult == null || lastResult.successfulInputs != result.successfulInputs)
                    {
                        lastResult = result;
                        Debug.Log($"BossQTEListener: QTE progress - {result.successfulInputs}/{result.totalInputs}");
                    }
                    
                    bool qteCompleted = (result.totalInputs > 0 && result.successfulInputs >= result.totalInputs) ||
                                       (result.totalInputs > 0 && result.accuracyPercent > 0 && result.damageMultiplier != 0);
                    
                    if (qteCompleted && (lastResult == null || lastResult.successfulInputs != result.successfulInputs || lastResult.damageMultiplier != result.damageMultiplier))
                    {
                        Debug.Log($"BossQTEListener: QTE completed. Damage calculation handled by JoustController.");
                        hasCalculatedDamage = true;
                        break;
                    }
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (checkCount > 1000)
            {
                Debug.LogWarning("BossQTEListener: Monitor timeout after 100 seconds, stopping");
                break;
            }
        }
        
        if (hasCalculatedDamage)
        {
            Debug.Log("BossQTEListener: Damage calculated successfully, destroying listener");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("BossQTEListener: Stopped monitoring without calculating damage");
            Destroy(gameObject);
        }
    }
}

