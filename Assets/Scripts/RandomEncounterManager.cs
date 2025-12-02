using UnityEngine;
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
        // StartNewRound(); // Wait for button press
    }

    public void StartNewRound()
    {
        StartCoroutine(StartNewRoundCoroutine());
    }
    
    private IEnumerator StartNewRoundCoroutine()
    {
        
        if (GameLoopManager.Instance == null)
        {
            Debug.LogError("GameLoopManager.Instance is null!");
            yield break;
        }
        
        if (GameLoopManager.Instance.isGameOver) 
        {
            yield break;
        }

        int currentRound = GameLoopManager.Instance.roundsCompleted;

        // Reset player/enemy positions at round start
        if (joustController != null)
        {
            joustController.ResetPositions();
        }
        
        // Show special message at round 4 (when tutorial ends)
        if (currentRound == 4)
        {
            GameLoopManager.Instance.ShowCenterMessage("Here goes more challenge!", 3f);
            yield return new WaitForSeconds(3f);
        }

        // Pick QTE type - first 4 rounds are tutorial (one of each type)
        QTEType[] qteTypes = new QTEType[] 
        { 
            QTEType.Sequence,      // Round 0: One key at a time
            QTEType.ComboInput,    // Round 1: Helldivers 2 style combo
            QTEType.ButtonMash,    // Round 2: Rapid mashing
            QTEType.HoldAndRelease // Round 3: Timing challenge
        };
        
        QTEType selectedType;
        if (currentRound < 4)
        {
            // Tutorial rounds: show each QTE type once in order
            selectedType = qteTypes[currentRound];
        }
        else
        {
            // After tutorial: random selection
            selectedType = qteTypes[Random.Range(0, qteTypes.Length)];
        }
        
        lastQTEType = selectedType; // Track for completion callback
        
        // Generate dynamic pattern based on round
        QTEPattern dynamicPattern = GeneratePattern(selectedType);
        
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
                // One key at a time: Start with 3 keys, add 1 every 2 rounds
                int keyCount = 3 + (currentRound / 2);
                pattern.inputSequence = GenerateRandomInputSequence(keyCount, type);
                pattern.minimumSuccessfulInputs = Mathf.Max(1, keyCount - 1); // Allow 1 mistake
                break;
            
            case QTEType.ComboInput:
                // Helldivers 2 style: Start with 3 keys, add 1 every 2 rounds
                int comboCount = 3 + (currentRound / 2);
                float comboTime = 4f - Mathf.Min(currentRound * 0.15f, 1.5f); // Min 2.5s
                pattern.inputSequence = GenerateRandomInputSequence(comboCount, type);
                pattern.inputSequence[0].windowDuration = Mathf.Max(comboTime, 2.5f);
                pattern.minimumSuccessfulInputs = comboCount; // Must complete entire combo
                break;
                
            case QTEType.ButtonMash:
                // Tutorial (rounds 0-3): always 6 presses
                // After tutorial (round 4+): 6 + number of Button Mash completions
                int requiredPresses;
                if (currentRound < 4)
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
                // Growth speed increases with rounds (shorter duration = faster ring growth)
                // Tutorial: 2.0s growth time, scales down to minimum 0.8s
                pattern.gapBetweenInputs = 2.0f - Mathf.Min(currentRound * 0.15f, 1.2f);
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
        
        // First 4 rounds (tutorial): arrow keys only
        if (currentRound < 4)
        {
            return arrowKeys[Random.Range(0, arrowKeys.Length)];
        }
        
        // After tutorial (round 4+): all keys
        return allKeys[Random.Range(0, allKeys.Length)];
    }
    
    // Called when a QTE is successfully completed
    public void OnQTECompleted()
    {
        // Only increment Button Mash counter after tutorial (round 4+)
        if (lastQTEType == QTEType.ButtonMash && GameLoopManager.Instance.roundsCompleted >= 4)
        {
            buttonMashCompletions++;
            // Track Button Mash completion count
        }
    }
    
    // Get hint text based on QTE type
    private string GetQTEHint(QTEType qteType)
    {
        int currentRound = GameLoopManager.Instance.roundsCompleted;
        
        switch (qteType)
        {
            case QTEType.Sequence:
                return currentRound < 4 ? "Press Arrow Keys One by One!" : "Press Keys One by One!";
            case QTEType.ComboInput:
                return currentRound < 4 ? "Input Full Arrow Combo!" : "Input Full Key Combo!";
            case QTEType.ButtonMash:
                return currentRound < 4 ? "Mash the Arrow Key Fast!" : "Mash the Key Fast!";
            case QTEType.HoldAndRelease:
                return currentRound < 4 ? "Hold & Release Arrow Key!" : "Hold & Release in Ring!";
            default:
                return "Get Ready!";
        }
    }
}
