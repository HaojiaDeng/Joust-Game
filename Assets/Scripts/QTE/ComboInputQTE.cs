using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComboInputQTE : BaseQTE
{
    // UI state for OnGUI
    private bool showUI = false;
    private List<KeyCode> comboKeys = new List<KeyCode>();
    private int currentIndex = 0;
    private float timeRemaining = 0f;
    private float totalTime = 0f;
    
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        // Extract all keys from the pattern
        comboKeys.Clear();
        foreach (QTEInput input in pattern.inputSequence)
        {
            comboKeys.Add(input.requiredKey);
        }
        
        currentIndex = 0;
        totalTime = pattern.inputSequence[0].windowDuration;
        timeRemaining = totalTime;
        showUI = true;
        
        result.totalInputs = comboKeys.Count;
        result.successfulInputs = 0;
        
        float elapsed = 0f;
        bool failed = false;
        
        while (elapsed < totalTime && currentIndex < comboKeys.Count && !failed)
        {
            timeRemaining = totalTime - elapsed;
            
            // Check if correct key is pressed
            if (Input.GetKeyDown(comboKeys[currentIndex]))
            {
                result.successfulInputs++;
                currentIndex++;
                
                // Brief success feedback
                if (GameLoopManager.Instance != null && currentIndex < comboKeys.Count)
                {
                    // Don't show message for intermediate keys, just for completion
                }
            }
            else if (Input.anyKeyDown)
            {
                // Wrong key pressed - check if it's an actual game key
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key) && key != comboKeys[currentIndex])
                    {
                        // Check if it's a relevant key (not mouse or modifier)
                        if (IsGameKey(key))
                        {
                            failed = true;
                            break;
                        }
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        showUI = false;
        ui.HidePrompt();
        
        CalculateFinalResult(pattern, result);
        
        // Show result
        if (currentIndex >= comboKeys.Count)
        {
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.ShowCenterMessage("PERFECT COMBO!", 0.5f);
            }
        }
        else if (failed)
        {
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.TakeDamage();
                GameLoopManager.Instance.ShowCenterMessage("WRONG KEY!", 0.5f);
            }
        }
        else
        {
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.TakeDamage();
                GameLoopManager.Instance.ShowCenterMessage("TOO SLOW!", 0.5f);
            }
        }
    }
    
    private bool IsGameKey(KeyCode key)
    {
        // Check if it's a relevant game key (not mouse buttons, modifiers, etc.)
        return (key >= KeyCode.A && key <= KeyCode.Z) ||
               (key >= KeyCode.UpArrow && key <= KeyCode.LeftArrow) ||
               key == KeyCode.Space;
    }
    
    private void OnGUI()
    {
        if (!showUI) return;
        
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f + 50f;
        
        // Background panel
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.7f));
        bgTex.Apply();
        
        // Calculate panel width based on number of keys
        float keyWidth = 60f;
        float keySpacing = 10f;
        float totalWidth = (comboKeys.Count * keyWidth) + ((comboKeys.Count - 1) * keySpacing) + 40f;
        float panelHeight = 120f;
        
        GUI.DrawTexture(new Rect(centerX - totalWidth/2, centerY - 60, totalWidth, panelHeight), bgTex);
        
        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.yellow;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(centerX - totalWidth/2, centerY - 55, totalWidth, 30), "INPUT COMBO!", titleStyle);
        
        // Draw each key in the combo
        float startX = centerX - (comboKeys.Count * keyWidth + (comboKeys.Count - 1) * keySpacing) / 2f;
        
        for (int i = 0; i < comboKeys.Count; i++)
        {
            float keyX = startX + i * (keyWidth + keySpacing);
            float keyY = centerY - 10;
            
            // Key background
            Color keyColor;
            if (i < currentIndex)
            {
                keyColor = new Color(0f, 0.8f, 0f, 1f); // Green - completed
            }
            else if (i == currentIndex)
            {
                keyColor = new Color(1f, 1f, 0f, 1f); // Yellow - current
            }
            else
            {
                keyColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Gray - pending
            }
            
            Texture2D keyBgTex = new Texture2D(1, 1);
            keyBgTex.SetPixel(0, 0, keyColor);
            keyBgTex.Apply();
            GUI.DrawTexture(new Rect(keyX, keyY, keyWidth, keyWidth), keyBgTex);
            
            // Key text
            GUIStyle keyStyle = new GUIStyle();
            keyStyle.fontSize = 24;
            keyStyle.fontStyle = FontStyle.Bold;
            keyStyle.normal.textColor = i < currentIndex ? Color.white : Color.black;
            keyStyle.alignment = TextAnchor.MiddleCenter;
            
            string keyText = GetKeyDisplayName(comboKeys[i]);
            GUI.Label(new Rect(keyX, keyY, keyWidth, keyWidth), keyText, keyStyle);
        }
        
        // Countdown timer
        GUIStyle timerStyle = new GUIStyle();
        timerStyle.fontSize = 28;
        timerStyle.fontStyle = FontStyle.Bold;
        timerStyle.normal.textColor = timeRemaining < 1f ? Color.red : Color.white;
        timerStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(centerX - 50, centerY + 35, 100, 35), timeRemaining.ToString("F1") + "s", timerStyle);
    }
    
    private string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.Space: return "SPC";
            default: return key.ToString();
        }
    }
}
