using UnityEngine;
using System.Collections;

public class JoustController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform enemy;
    [SerializeField] private Camera mainCamera;
    
    private CardData currentCard;
    
    // Starting positions for reset
    private Vector3 playerStartPos = new Vector3(-5f, 0f, 0f);
    private Vector3 enemyStartPos = new Vector3(5f, 0f, 0f);
    
    public void ResetPositions()
    {
        if (player != null)
        {
            // Check if it's RectTransform (UI element) or Transform (world space)
            RectTransform playerRect = player.GetComponent<RectTransform>();
            if (playerRect != null)
            {
                // It's a UI element - use localPosition
                playerRect.localPosition = playerStartPos;
                Debug.Log($"Player (RectTransform) reset to localPosition: {playerStartPos}");
            }
            else
            {
                // It's a world space object - use position
                player.position = playerStartPos;
                Debug.Log($"Player (Transform) reset to position: {playerStartPos}");
            }
            
            // Ensure Player is visible and in front of BG
            if (!player.gameObject.activeInHierarchy)
            {
                player.gameObject.SetActive(true);
            }
            // Set sorting layer to be above BG
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.sortingLayerName = "Default";
                playerSprite.sortingOrder = 10; // Above BG (which should be lower)
                playerSprite.enabled = true;
            }
        }
        if (enemy != null)
        {
            // Check if it's RectTransform (UI element) or Transform (world space)
            RectTransform enemyRect = enemy.GetComponent<RectTransform>();
            if (enemyRect != null)
            {
                // It's a UI element - use localPosition
                enemyRect.localPosition = enemyStartPos;
                Debug.Log($"Enemy (RectTransform) reset to localPosition: {enemyStartPos}");
            }
            else
            {
                // It's a world space object - use position
                enemy.position = enemyStartPos;
                Debug.Log($"Enemy (Transform) reset to position: {enemyStartPos}");
            }
            
            // Ensure Enemy is visible and in front of BG
            if (!enemy.gameObject.activeInHierarchy)
            {
                enemy.gameObject.SetActive(true);
            }
            // Set sorting layer to be above BG
            SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();
            if (enemySprite != null)
            {
                enemySprite.sortingLayerName = "Default";
                enemySprite.sortingOrder = 10; // Above BG (which should be lower)
                enemySprite.enabled = true;
            }
        }
    }
    
    private QTEManager GetQTEManager()
    {
        QTEManager qteManager = GetComponent<QTEManager>();
        if (qteManager == null)
        {
            qteManager = GetComponentInChildren<QTEManager>(true);
        }
        if (qteManager == null)
        {
            qteManager = GetComponentInParent<QTEManager>();
        }
        if (qteManager == null)
        {
            qteManager = FindFirstObjectByType<QTEManager>();
        }
        return qteManager;
    }
    
    public void StartJoust(CardData card)
    {
        Debug.Log("StartJoust called!");
        currentCard = card;
        
        // Ensure Player and Enemy references are set
        // Try to find as children of this GameObject (QTE UI)
        if (player == null)
        {
            // First try to find as child
            Transform playerTransform = transform.Find("Player");
            if (playerTransform != null)
            {
                player = playerTransform;
                Debug.Log("Found Player as child of QTE UI");
            }
            else
            {
                // Fallback: search by name
                GameObject playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    Debug.Log("Found Player by name search");
                }
            }
        }
        
        if (enemy == null)
        {
            // First try to find as child
            Transform enemyTransform = transform.Find("Enemy");
            if (enemyTransform != null)
            {
                enemy = enemyTransform;
                Debug.Log("Found Enemy as child of QTE UI");
            }
            else
            {
                // Fallback: search by name
                GameObject enemyObj = GameObject.Find("Enemy");
                if (enemyObj != null)
                {
                    enemy = enemyObj.transform;
                    Debug.Log("Found Enemy by name search");
                }
            }
        }
        
        // Ensure camera is set up
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            // Ensure camera can see default layer (where Player/Enemy might be)
            int defaultLayer = LayerMask.NameToLayer("Default");
            if (defaultLayer >= 0)
            {
                mainCamera.cullingMask |= (1 << defaultLayer);
            }
            
            // For 2D sprites, ensure camera is set to Orthographic
            if (!mainCamera.orthographic)
            {
                Debug.LogWarning("Camera is not orthographic! Setting to orthographic for 2D rendering.");
                mainCamera.enabled = true;
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 5f; // Adjust as needed
            }
            
            // Ensure camera is at proper Z position to see sprites at Z=0
            if (mainCamera.transform.position.z > -10)
            {
                mainCamera.transform.position = new Vector3(0, 0, -10);
                Debug.Log("Camera position adjusted to see 2D sprites");
            }
            
            // Ensure camera is definitely enabled
            mainCamera.enabled = true;
            
            Debug.Log($"Main Camera enabled: {mainCamera.name}, Orthographic: {mainCamera.orthographic}, Position: {mainCamera.transform.position}, CullingMask: {mainCamera.cullingMask}, Enabled: {mainCamera.enabled}");
        }
        else
        {
            Debug.LogError("No camera found for QTE!");
        }
        
        ResetPositions();
        
        StartCoroutine(JoustSequence());
    }
    
    private IEnumerator JoustSequence()
    {
        Debug.Log("JoustSequence started!");
        
        // Ensure prompt is hidden during charge animation
        QTEUIController uiController = GetComponentInChildren<QTEUIController>(true);
        if (uiController == null)
        {
            GameObject promptPanel = GameObject.Find("PromptPanel");
            if (promptPanel != null)
            {
                uiController = promptPanel.GetComponent<QTEUIController>();
            }
        }
        if (uiController != null)
        {
            uiController.HidePrompt();
            Debug.Log("Prompt hidden before charge animation");
        }
        
        // Play charge animations
        yield return StartCoroutine(ChargeSequence());
        
        Debug.Log("ChargeSequence complete, starting QTE...");
        
        QTEManager qteManager = GetQTEManager();
        if (qteManager == null)
        {
            Debug.LogError("QTEManager not found!");
            yield break;
        }
        
        Debug.Log($"QTEManager found: {qteManager.name} on {qteManager.gameObject.name}");
        
        // Get current round number to determine how many QTE rounds to execute
        int currentRound = GameLoopManager.Instance != null ? GameLoopManager.Instance.roundsCompleted : 1;
        if (currentRound < 1) currentRound = 1;
        
        Debug.Log($"Current round: {currentRound}, will execute QTE {currentRound} time(s)");
        
        // Get RandomEncounterManager for generating random QTE patterns
        RandomEncounterManager randomEncounter = GetComponentInChildren<RandomEncounterManager>(true);
        if (randomEncounter == null)
        {
            GameObject qteUI = GameObject.Find("QTE");
            if (qteUI != null)
            {
                randomEncounter = qteUI.GetComponentInChildren<RandomEncounterManager>(true);
            }
        }
        if (randomEncounter == null)
        {
            randomEncounter = FindFirstObjectByType<RandomEncounterManager>();
        }
        
        if (randomEncounter == null)
        {
            Debug.LogError("RandomEncounterManager not found! Cannot generate random QTE patterns.");
            yield break;
        }
        
        QTEResult finalResult = new QTEResult();
        finalResult.totalInputs = 0;
        finalResult.successfulInputs = 0;
        finalResult.damageMultiplier = 0f;
        bool allRoundsSuccess = true;
        float totalDamageMultiplier = 0f;
        
        // Execute QTE multiple times based on round number, each with a random QTE type
        for (int round = 1; round <= currentRound; round++)
        {
            Debug.Log($"QTE Round {round}/{currentRound} starting...");
            
            // Generate a random QTE pattern for this round
            QTEPattern randomPattern = randomEncounter.GenerateRandomQTEPattern();
            if (randomPattern == null)
            {
                Debug.LogError($"Failed to generate random QTE pattern for round {round}!");
                allRoundsSuccess = false;
                break;
            }
            
            Debug.Log($"QTE Round {round}/{currentRound}: Using random pattern type: {randomPattern.qteType}");
            
            yield return StartCoroutine(qteManager.ExecuteQTE(randomPattern));
            QTEResult roundResult = qteManager.GetLastQTEResult();
            
            if (roundResult == null)
            {
                Debug.LogError($"QTEResult is null after QTE round {round}!");
                allRoundsSuccess = false;
                break;
            }
            
            Debug.Log($"QTE Round {round}/{currentRound} complete. Result: {roundResult.successfulInputs}/{roundResult.totalInputs}, Multiplier: {roundResult.damageMultiplier}");
            
            // Accumulate results
            finalResult.totalInputs += roundResult.totalInputs;
            finalResult.successfulInputs += roundResult.successfulInputs;
            totalDamageMultiplier += roundResult.damageMultiplier;
            
            // Check if this round failed
            if (roundResult.damageMultiplier <= 0f || roundResult.successfulInputs == 0)
            {
                Debug.Log($"QTE Round {round}/{currentRound} FAILED! Breaking out of loop.");
                allRoundsSuccess = false;
                finalResult.damageMultiplier = 0f;
                finalResult.successfulInputs = 0;
                break;
            }
            
            // Small delay between rounds (except after the last round)
            if (round < currentRound)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        // Calculate average damage multiplier if all rounds succeeded
        if (allRoundsSuccess && currentRound > 0)
        {
            finalResult.damageMultiplier = totalDamageMultiplier / currentRound;
            Debug.Log($"All {currentRound} QTE rounds succeeded! Average damage multiplier: {finalResult.damageMultiplier}");
        }
        else
        {
            finalResult.damageMultiplier = 0f;
            finalResult.successfulInputs = 0;
            Debug.Log($"QTE FAILED! Not all rounds succeeded.");
        }
        
        Debug.Log($"Final QTE Result: {finalResult.successfulInputs}/{finalResult.totalInputs}, Multiplier: {finalResult.damageMultiplier}");
        
        // Check final result
        if (finalResult.damageMultiplier <= 0f || finalResult.successfulInputs == 0)
        {
            Debug.Log($"QTE FAILED! Damage Multiplier: {finalResult.damageMultiplier}, Successful Inputs: {finalResult.successfulInputs}");
            yield return new WaitForSeconds(1f);
            StartCoroutine(EndQTEFailed());
        }
        else
        {
            Debug.Log($"QTE SUCCESS! Damage Multiplier: {finalResult.damageMultiplier}, Successful Inputs: {finalResult.successfulInputs}");
            yield return new WaitForSeconds(1f);
            StartCoroutine(EndQTESuccess(finalResult));
        }
    }
    
    private IEnumerator ChargeSequence()
    {
        Debug.Log("ChargeSequence: Starting charge animation");
        
        // Ensure Player and Enemy are set
        if (player == null)
        {
            // Try to find as child first
            Transform playerTransform = transform.Find("Player");
            if (playerTransform != null)
            {
                player = playerTransform;
                Debug.Log("Found Player as child in ChargeSequence");
            }
            else
            {
                GameObject playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    Debug.Log("Found Player by name in ChargeSequence");
                }
                else
                {
                    Debug.LogError("Player GameObject not found!");
                    yield break;
                }
            }
        }
        
        if (enemy == null)
        {
            // Try to find as child first
            Transform enemyTransform = transform.Find("Enemy");
            if (enemyTransform != null)
            {
                enemy = enemyTransform;
                Debug.Log("Found Enemy as child in ChargeSequence");
            }
            else
            {
                GameObject enemyObj = GameObject.Find("Enemy");
                if (enemyObj != null)
                {
                    enemy = enemyObj.transform;
                    Debug.Log("Found Enemy by name in ChargeSequence");
                }
                else
                {
                    Debug.LogError("Enemy GameObject not found!");
                    yield break;
                }
            }
        }
        
        // Ensure they are active
        if (!player.gameObject.activeInHierarchy)
        {
            player.gameObject.SetActive(true);
            Debug.Log("Player activated");
        }
        if (!enemy.gameObject.activeInHierarchy)
        {
            enemy.gameObject.SetActive(true);
            Debug.Log("Enemy activated");
        }
        
        // Use current positions (already reset by ResetPositions)
        Vector3 playerStart = player.position;
        Vector3 enemyStart = enemy.position;
        Vector3 meetPoint = (playerStart + enemyStart) / 2;
        
        Debug.Log($"Player start: {playerStart}, Enemy start: {enemyStart}, Meet point: {meetPoint}");
        
        float duration = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            player.position = Vector3.Lerp(playerStart, meetPoint + Vector3.left * 0.5f, t);
            enemy.position = Vector3.Lerp(enemyStart, meetPoint + Vector3.right * 0.5f, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("ChargeSequence: Animation complete");
    }
    
    private IEnumerator EndQTESuccess(QTEResult result)
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("EndQTESuccess: Starting cleanup...");

        if (CardManagement.Instance != null)
        {
            CardManagement.Instance.OnQTESuccess(currentCard, result);
        }
        
        Debug.Log("EndQTESuccess: QTE completed successfully");
    }

    private IEnumerator EndQTEFailed()
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("EndQTEFailed: Starting cleanup...");

        if (CardManagement.Instance != null)
        {
            Debug.Log($"EndQTEFailed: Calling CardManagement.OnQTEFailed with card: {currentCard?.cardName ?? "null"}");
            CardManagement.Instance.OnQTEFailed(currentCard);
        }
        else
        {
            Debug.LogError("EndQTEFailed: CardManagement.Instance is NULL! Cannot call OnQTEFailed.");
        }
        
        Debug.Log("EndQTEFailed: QTE failed and cleaned up - returning to card playing");
    }

    private string GetQTEHint(QTEType qteType)
    {
        int currentRound = GameLoopManager.Instance != null ? GameLoopManager.Instance.roundsCompleted : 0;
        
        switch (qteType)
        {
            case QTEType.Directional:
                return currentRound < 6 ? "Press Arrow Keys!" : "Press the Keys!";
            case QTEType.ButtonMash:
                return "Mash the Key Fast!";
            case QTEType.Sequence:
                return "Press Keys in Order!";
            case QTEType.Rhythm:
                return "Press on the Beat!";
            case QTEType.HoldAndRelease:
                return "Hold & Release in Ring!";
            default:
                return "Get Ready!";
        }
    }

}
