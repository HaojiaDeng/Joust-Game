using UnityEngine;
using System.Collections;

public class ButtonMashQTE : BaseQTE
{
    // UI state for OnGUI
    private bool showUI = false;
    private float progress = 0f;        // 0 to 1
    private float timeRemaining = 0f;   // Countdown
    private float totalTime = 0f;
    private string currentKeyText = "";
    private int currentPresses = 0;
    private int targetPresses = 0;
    
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        // For button mash, we use the first input in the sequence
        QTEInput mashInput = pattern.inputSequence[0];
        KeyCode mashKey = mashInput.requiredKey;
        float duration = mashInput.windowDuration;
        
        targetPresses = pattern.minimumSuccessfulInputs;
        currentPresses = 0;
        currentKeyText = mashKey.ToString();
        totalTime = duration;
        timeRemaining = duration;
        progress = 0f;
        showUI = true;
        
        result.totalInputs = targetPresses;
        
        float elapsed = 0f;
        bool lastFramePressed = false;
        
        while (elapsed < duration)
        {
            // Update UI state
            timeRemaining = duration - elapsed;
            progress = (float)currentPresses / targetPresses;
            
            // Detect key press (prevent holding)
            bool currentlyPressed = Input.GetKey(mashKey);
            
            if (currentlyPressed && !lastFramePressed)
            {
                currentPresses++;
                ui.ShowSuccess();
            }
            else if (Input.anyKeyDown && !currentlyPressed)
            {
                // Check if wrong key was pressed - deal damage
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key) && key != mashKey && IsGameKey(key))
                    {
                        if (GameLoopManager.Instance != null)
                        {
                            GameLoopManager.Instance.TakeDamage();
                            GameLoopManager.Instance.ShowCenterMessage("WRONG KEY!", 0.3f);
                        }
                        break;
                    }
                }
            }
            
            lastFramePressed = currentlyPressed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        showUI = false;
        ui.HidePrompt();
        
        result.successfulInputs = Mathf.Min(currentPresses, targetPresses);
        CalculateFinalResult(pattern, result);

        // Check success or failure
        if (currentPresses >= targetPresses)
        {
            if (GameLoopManager.Instance != null) 
            {
                GameLoopManager.Instance.ShowCenterMessage("SUCCESS!", 0.5f);
            }
        }
        else
        {
            if (GameLoopManager.Instance != null) 
            {
                GameLoopManager.Instance.TakeDamage();
                GameLoopManager.Instance.ShowCenterMessage("FAILED!", 0.5f);
            }
        }
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
        GUI.DrawTexture(new Rect(centerX - 150, centerY - 80, 300, 160), bgTex);
        
        // Key prompt
        GUIStyle keyStyle = new GUIStyle();
        keyStyle.fontSize = 40;
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.normal.textColor = Color.white;
        keyStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(centerX - 100, centerY - 70, 200, 50), currentKeyText, keyStyle);
        
        // "MASH!" text
        GUIStyle mashStyle = new GUIStyle();
        mashStyle.fontSize = 20;
        mashStyle.normal.textColor = Color.yellow;
        mashStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(centerX - 100, centerY - 25, 200, 30), "MASH IT!", mashStyle);
        
        // Progress bar background
        Texture2D barBgTex = new Texture2D(1, 1);
        barBgTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
        barBgTex.Apply();
        
        float barWidth = 250f;
        float barHeight = 25f;
        float barX = centerX - barWidth / 2f;
        float barY = centerY + 10f;
        
        GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight), barBgTex);
        
        // Progress bar fill
        Texture2D barFillTex = new Texture2D(1, 1);
        Color barColor = progress >= 1f ? Color.green : Color.cyan;
        barFillTex.SetPixel(0, 0, barColor);
        barFillTex.Apply();
        
        float fillWidth = Mathf.Min(progress, 1f) * barWidth;
        GUI.DrawTexture(new Rect(barX, barY, fillWidth, barHeight), barFillTex);
        
        // Progress text
        GUIStyle progressStyle = new GUIStyle();
        progressStyle.fontSize = 16;
        progressStyle.fontStyle = FontStyle.Bold;
        progressStyle.normal.textColor = Color.white;
        progressStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(barX, barY, barWidth, barHeight), $"{currentPresses} / {targetPresses}", progressStyle);
        
        // Countdown timer
        GUIStyle timerStyle = new GUIStyle();
        timerStyle.fontSize = 28;
        timerStyle.fontStyle = FontStyle.Bold;
        timerStyle.normal.textColor = timeRemaining < 1f ? Color.red : Color.white;
        timerStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(centerX - 50, centerY + 45, 100, 35), timeRemaining.ToString("F1") + "s", timerStyle);
    }
    
    private bool IsGameKey(KeyCode key)
    {
        // Check if it's a relevant game key (not mouse buttons, modifiers, etc.)
        return (key >= KeyCode.A && key <= KeyCode.Z) ||
               (key >= KeyCode.UpArrow && key <= KeyCode.LeftArrow) ||
               key == KeyCode.Space;
    }
}