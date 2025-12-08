using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RandomEncounterManager : MonoBehaviour
{
    public JoustController joustController;
    
    // Track Button Mash completions for difficulty scaling
    private int buttonMashCompletions = 0;
    private QTEType lastQTEType;
    
    // Arrow keys only (for early rounds)
    private static readonly KeyCode[] arrowKeys = new KeyCode[]
    {
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow
    };
    
    // All keys (after round 6)
    private static readonly KeyCode[] allKeys = new KeyCode[]
    {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
        KeyCode.Space
    };

    private void Start()
    {
        if (gameObject.activeInHierarchy && enabled)
        {
            StartCoroutine(AutoStartFirstRound());
        }
    }

    private IEnumerator AutoStartFirstRound()
    {
        yield return null;
        if (enabled && gameObject.activeInHierarchy)
        {
            StartNewRound();
        }
    }

    public void StartNewRound()
    {
        StartCoroutine(StartNewRoundCoroutine());
    }
    
    private IEnumerator StartNewRoundCoroutine()
    {
        Debug.Log("StartNewRound called.");
        
        if (GameLoopManager.Instance == null)
        {
            Debug.LogError("GameLoopManager.Instance is null!");
            yield break;
        }
        
        if (GameLoopManager.Instance.isGameOver) 
        {
            Debug.Log("StartNewRound: Game is over, skipping.");
            yield break;
        }

        int currentRound = GameLoopManager.Instance.roundsCompleted;

        // Reset player/enemy positions at round start
        if (joustController != null)
        {
            joustController.ResetPositions();
        }
        
        // Show special message at round 6
        if (currentRound == 6)
        {
            GameLoopManager.Instance.ShowCenterMessage("Here goes more challenge!", 3f);
            yield return new WaitForSeconds(3f);
        }

        // Pick random QTE type
        QTEType[] qteTypes = new QTEType[] 
        { 
            QTEType.Sequence, 
            QTEType.Directional, 
            QTEType.Rhythm,
            QTEType.ButtonMash,
            QTEType.HoldAndRelease
        };
        QTEType selectedType = qteTypes[Random.Range(0, qteTypes.Length)];
        lastQTEType = selectedType; // Track for completion callback
        
        // Generate dynamic pattern based on round
        QTEPattern dynamicPattern = GeneratePattern(selectedType);
        Debug.Log($"StartNewRound: Selected QTE type {selectedType}, Round: {GameLoopManager.Instance.roundsCompleted}");
        
        // Show hint for current QTE type
        if (GameLoopManager.Instance != null)
        {
            string hint = GetQTEHint(selectedType);
            GameLoopManager.Instance.ShowCenterMessage(hint, 2f);
        }
        
        // Create temp card data
        CardData tempCard = ScriptableObject.CreateInstance<CardData>();
        tempCard.qtePattern = dynamicPattern;

        // Start game
        if (joustController != null)
        {
            joustController.StartJoust(tempCard);
        }
        else
        {
            Debug.LogError("StartNewRound: JoustController is null!");
        }
    }
    
    public QTEPattern GenerateRandomQTEPattern()
    {
        QTEType[] qteTypes = new QTEType[] 
        { 
            QTEType.Sequence, 
            QTEType.Directional, 
            QTEType.Rhythm,
            QTEType.ButtonMash,
            QTEType.HoldAndRelease
        };
        QTEType selectedType = qteTypes[Random.Range(0, qteTypes.Length)];
        lastQTEType = selectedType;
        
        return GeneratePattern(selectedType);
    }
    
    private QTEPattern GeneratePattern(QTEType type)
    {
        QTEPattern pattern = ScriptableObject.CreateInstance<QTEPattern>();
        pattern.qteType = type;
        pattern.gapBetweenInputs = 0.5f;
        pattern.mustBeConsecutive = false;
        
        int currentRound = GameLoopManager.Instance.roundsCompleted;
        
        switch (type)
        {
            case QTEType.Sequence:
            case QTEType.Directional:
            case QTEType.Rhythm:
                // Start with 3 keys, add 1 every 2 rounds
                int keyCount = 3 + (currentRound / 2);
                pattern.inputSequence = GenerateRandomInputSequence(keyCount, type);
                pattern.minimumSuccessfulInputs = Mathf.Max(1, keyCount - 1); // Allow 1 mistake
                break;
                
            case QTEType.ButtonMash:
                // Before round 6: always 6 presses
                // After round 6: 6 + number of Button Mash completions
                int requiredPresses;
                if (currentRound < 6)
                {
                    requiredPresses = 6;
                }
                else
                {
                    requiredPresses = 6 + buttonMashCompletions;
                }
                float mashDuration = 3f - Mathf.Min(currentRound * 0.1f, 1f); // Reduce time, min 2s
                pattern.inputSequence = new List<QTEInput>
                {
                    new QTEInput
                    {
                        requiredKey = GetRandomKey(),
                        windowDuration = Mathf.Max(mashDuration, 2f),
                        delayBeforeThisPrompt = 0.5f
                    }
                };
                pattern.minimumSuccessfulInputs = requiredPresses;
                break;
                
            case QTEType.HoldAndRelease:
                // Reduce timing window as rounds progress
                float holdWindow = 3f - Mathf.Min(currentRound * 0.15f, 1.2f); // Min 1.8s window
                pattern.inputSequence = new List<QTEInput>
                {
                    new QTEInput
                    {
                        requiredKey = GetRandomKey(),
                        windowDuration = Mathf.Max(holdWindow, 1.8f),
                        delayBeforeThisPrompt = 0.5f
                    }
                };
                pattern.minimumSuccessfulInputs = 1;
                // Adjust perfect zone in the QTE executor based on round
                pattern.gapBetweenInputs = 2.0f - Mathf.Min(currentRound * 0.05f, 0.5f); // Faster growth
                break;
        }
        
        return pattern;
    }
    
    private List<QTEInput> GenerateRandomInputSequence(int count, QTEType type)
    {
        List<QTEInput> inputs = new List<QTEInput>();
        
        // Base timing that gets slightly faster with rounds
        int currentRound = GameLoopManager.Instance.roundsCompleted;
        float baseWindow = 1.2f - Mathf.Min(currentRound * 0.03f, 0.4f); // Min 0.8s window
        
        for (int i = 0; i < count; i++)
        {
            QTEInput input = new QTEInput
            {
                requiredKey = GetRandomKey(),
                windowDuration = Mathf.Max(baseWindow, 0.8f),
                delayBeforeThisPrompt = i == 0 ? 0.5f : 0f
            };
            inputs.Add(input);
        }
        
        return inputs;
    }
    
    private KeyCode GetRandomKey()
    {
        int currentRound = GameLoopManager.Instance.roundsCompleted;
        
        // First 6 rounds: arrow keys only
        if (currentRound < 6)
        {
            return arrowKeys[Random.Range(0, arrowKeys.Length)];
        }
        
        // After round 6: all keys
        return allKeys[Random.Range(0, allKeys.Length)];
    }
    
    // Called when a QTE is successfully completed
    public void OnQTECompleted()
    {
        // Only increment Button Mash counter after round 6
        if (lastQTEType == QTEType.ButtonMash && GameLoopManager.Instance.roundsCompleted >= 6)
        {
            buttonMashCompletions++;
            Debug.Log($"Button Mash completed! Total completions: {buttonMashCompletions}");
        }
    }
    
    // Get hint text based on QTE type
    private string GetQTEHint(QTEType qteType)
    {
        int currentRound = GameLoopManager.Instance.roundsCompleted;
        
        switch (qteType)
        {
            case QTEType.Directional:
                // Change hint after round 6 when new keys are introduced
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
