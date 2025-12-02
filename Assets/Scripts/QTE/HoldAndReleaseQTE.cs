using UnityEngine;
using System.Collections;

public class HoldAndReleaseQTE : BaseQTE
{
    // Ring visualization state (for OnGUI drawing)
    private bool showUI = false;
    private float innerRingProgress = 0f; // 0 to 1+
    private float perfectZoneStart = 0.85f;
    private float perfectZoneEnd = 1.0f;
    private bool isHolding = false;
    private string currentKeyText = "SPACE";
    private string statusText = "Press and HOLD the key!";
    private Texture2D whiteTex;
    
    private void Awake()
    {
        // Pre-create texture to avoid recreating in OnGUI
        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }
    
    public override IEnumerator Execute(QTEPattern pattern, QTEResult result, QTEUIController ui)
    {
        result.totalInputs = pattern.inputSequence.Count;
        result.successfulInputs = 0;
        
        foreach (QTEInput input in pattern.inputSequence)
        {
            yield return new WaitForSeconds(input.delayBeforeThisPrompt);
            
            currentKeyText = input.requiredKey.ToString();
            statusText = "Press and HOLD [" + currentKeyText + "]!";
            innerRingProgress = 0f;
            isHolding = false;
            showUI = true;  // Show UI from the start
            
            // Phase 1: Wait for player to start holding
            float waitTime = 0f;
            float maxWaitTime = input.windowDuration;
            bool wrongKeyPressed = false;
            
            while (waitTime < maxWaitTime && !isHolding && !wrongKeyPressed)
            {
                if (Input.GetKeyDown(input.requiredKey))
                {
                    isHolding = true;
                    statusText = "HOLD... Release in green zone!";
                }
                else if (Input.anyKeyDown)
                {
                    // Check if a wrong game key was pressed
                    foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(key) && key != input.requiredKey && IsGameKey(key))
                        {
                            wrongKeyPressed = true;
                            break;
                        }
                    }
                }
                waitTime += Time.deltaTime;
                yield return null;
            }
            
            if (wrongKeyPressed)
            {
                showUI = false;
                if (GameLoopManager.Instance != null) 
                {
                    GameLoopManager.Instance.TakeDamage();
                    GameLoopManager.Instance.ShowCenterMessage("WRONG KEY!", 0.5f);
                }
                yield return new WaitForSeconds(pattern.gapBetweenInputs);
                continue;
            }
            
            if (!isHolding)
            {
                showUI = false;
                if (GameLoopManager.Instance != null) 
                {
                    GameLoopManager.Instance.TakeDamage();
                    GameLoopManager.Instance.ShowCenterMessage("MISS! You didn't press!", 0.5f);
                }
                yield return new WaitForSeconds(pattern.gapBetweenInputs);
                continue;
            }
            
            // Phase 2: Ring grows while holding - release at right time!
            // Use pattern.gapBetweenInputs as growth duration (difficulty scaling)
            float growDuration = pattern.gapBetweenInputs;
            float growSpeed = 1f / growDuration;
            bool released = false;
            
            while (!released && innerRingProgress <= 1.15f)
            {
                if (Input.GetKey(input.requiredKey))
                {
                    innerRingProgress += growSpeed * Time.deltaTime;
                    
                    // Update status based on progress
                    if (innerRingProgress >= perfectZoneStart && innerRingProgress <= perfectZoneEnd)
                    {
                        statusText = "NOW! Release!";
                    }
                    else if (innerRingProgress > perfectZoneEnd)
                    {
                        statusText = "TOO LATE!";
                    }
                }
                
                if (Input.GetKeyUp(input.requiredKey))
                {
                    released = true;
                    showUI = false;
                    
                    if (innerRingProgress >= perfectZoneStart && innerRingProgress <= perfectZoneEnd)
                    {
                        result.successfulInputs++;
                        if (GameLoopManager.Instance != null)
                        {
                            GameLoopManager.Instance.ShowCenterMessage("PERFECT!", 0.5f);
                        }
                    }
                    else if (innerRingProgress < perfectZoneStart)
                    {
                        if (GameLoopManager.Instance != null)
                        {
                            GameLoopManager.Instance.TakeDamage();
                            GameLoopManager.Instance.ShowCenterMessage("TOO EARLY!", 0.5f);
                        }
                    }
                    else
                    {
                        if (GameLoopManager.Instance != null)
                        {
                            GameLoopManager.Instance.TakeDamage();
                            GameLoopManager.Instance.ShowCenterMessage("TOO LATE!", 0.5f);
                        }
                    }
                }
                
                yield return null;
            }
            
            if (!released)
            {
                showUI = false;
                if (GameLoopManager.Instance != null)
                {
                    GameLoopManager.Instance.TakeDamage();
                    GameLoopManager.Instance.ShowCenterMessage("TOO LATE!", 0.5f);
                }
            }
            
            isHolding = false;
            yield return new WaitForSeconds(pattern.gapBetweenInputs);
        }
        
        CalculateFinalResult(pattern, result);
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
        float centerY = Screen.height / 2f - 80f;  // Move up by 80px
        float outerRadius = 100f;
        float innerRadius = outerRadius * innerRingProgress;
        
        // Semi-transparent background circle
        DrawFilledCircle(centerX, centerY, outerRadius + 20f, new Color(0f, 0f, 0f, 0.5f));
        
        // Outer ring (target) - cyan
        DrawRingSimple(centerX, centerY, outerRadius, Color.cyan);
        
        // Perfect zone - green area
        DrawPerfectZone(centerX, centerY, outerRadius * perfectZoneStart, outerRadius, new Color(0f, 1f, 0f, 0.4f));
        
        // Inner ring (growing) - color based on progress
        if (isHolding && innerRingProgress > 0)
        {
            Color innerColor = Color.yellow;
            if (innerRingProgress >= perfectZoneStart && innerRingProgress <= perfectZoneEnd)
            {
                innerColor = Color.green;
            }
            else if (innerRingProgress > perfectZoneEnd)
            {
                innerColor = Color.red;
            }
            DrawRingSimple(centerX, centerY, innerRadius, innerColor);
        }
        
        // Display key in center
        GUIStyle keyStyle = new GUIStyle();
        keyStyle.fontSize = 36;
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.normal.textColor = Color.white;
        keyStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(centerX - 75, centerY - 25, 150, 50), currentKeyText, keyStyle);
        
        // Display status text below
        GUIStyle statusStyle = new GUIStyle();
        statusStyle.fontSize = 20;
        statusStyle.normal.textColor = Color.yellow;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(centerX - 200, centerY + outerRadius + 30, 400, 40), statusText, statusStyle);
    }
    
    private void DrawFilledCircle(float x, float y, float radius, Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        
        // Approximate circle with rectangles
        int steps = 20;
        for (int i = -steps; i <= steps; i++)
        {
            float yOffset = (i / (float)steps) * radius;
            float xHalf = Mathf.Sqrt(radius * radius - yOffset * yOffset);
            GUI.DrawTexture(new Rect(x - xHalf, y + yOffset - 2, xHalf * 2, 4), tex);
        }
    }
    
    private void DrawRingSimple(float x, float y, float radius, Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        
        int segments = 36;
        float thickness = 4f;
        
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float px = x + Mathf.Cos(angle) * radius;
            float py = y + Mathf.Sin(angle) * radius;
            GUI.DrawTexture(new Rect(px - thickness/2, py - thickness/2, thickness, thickness), tex);
        }
    }
    
    private void DrawPerfectZone(float x, float y, float innerR, float outerR, Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        
        int segments = 36;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float cosA = Mathf.Cos(angle);
            float sinA = Mathf.Sin(angle);
            
            // Draw lines from inner to outer ring
            for (float r = innerR; r <= outerR; r += 3f)
            {
                float px = x + cosA * r;
                float py = y + sinA * r;
                GUI.DrawTexture(new Rect(px - 2, py - 2, 4, 4), tex);
            }
        }
    }
}