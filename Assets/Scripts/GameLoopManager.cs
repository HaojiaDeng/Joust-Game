using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;
    public int maxHealth = 10;
    public int currentHealth;
    public bool isGameOver = false;
    public int roundsCompleted = 0;

    [Header("UI State")]
    public string centerText = "";
    public float centerTextTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        currentHealth = maxHealth;
    }

    public void TakeDamage()
    {
        if (isGameOver) return;
        currentHealth--;
        // Damage taken
        
        // Flash damage feedback
        ShowCenterMessage($"HP: {currentHealth}/{maxHealth}", 1f);
        
        if (currentHealth <= 0)
        {
            isGameOver = true;
            Time.timeScale = 0f; // Freeze game
            // Game Over - Game Frozen
        }
    }

    public void Heal()
    {
        if (isGameOver) return;
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            // Healed 1 HP
        }
    }

    // Advance round counter regardless of heal state
    public void AdvanceRound()
    {
        if (isGameOver) return;
        roundsCompleted++;
        // Round advanced
    }

    public void ShowCenterMessage(string message, float duration)
    {
        centerText = message;
        centerTextTimer = duration;
    }

    private void Update()
    {
        // Don't update timers when game is frozen
        if (isGameOver) return;
        
        if (centerTextTimer > 0)
        {
            centerTextTimer -= Time.deltaTime;
            if (centerTextTimer <= 0) centerText = "";
        }
    }

    private void OnGUI()
    {
        // Draw HP as large text in top-left
        float startX = 20f;
        float startY = 20f;
        
        // Determine color based on HP
        Color hpColor;
        float hpPercent = (float)currentHealth / maxHealth;
        if (hpPercent > 0.6f)
            hpColor = Color.white;  // High HP - white
        else if (hpPercent > 0.3f)
            hpColor = new Color(1f, 0.7f, 0f);  // Medium HP - orange
        else
            hpColor = Color.red;  // Low HP - red
        
        // HP text style
        GUIStyle hpTextStyle = new GUIStyle();
        hpTextStyle.fontSize = 48;  // Large font
        hpTextStyle.fontStyle = FontStyle.Bold;
        hpTextStyle.normal.textColor = hpColor;
        hpTextStyle.alignment = TextAnchor.UpperLeft;
        
        // Shadow for HP text (black outline)
        GUIStyle hpShadowStyle = new GUIStyle(hpTextStyle);
        hpShadowStyle.normal.textColor = Color.black;
        
        string hpText = $"HP: {currentHealth}/{maxHealth}";
        
        // Draw shadow in 4 directions for outline effect
        GUI.Label(new Rect(startX - 2, startY, 300, 60), hpText, hpShadowStyle);
        GUI.Label(new Rect(startX + 2, startY, 300, 60), hpText, hpShadowStyle);
        GUI.Label(new Rect(startX, startY - 2, 300, 60), hpText, hpShadowStyle);
        GUI.Label(new Rect(startX, startY + 2, 300, 60), hpText, hpShadowStyle);
        
        // Draw main HP text
        GUI.Label(new Rect(startX, startY, 300, 60), hpText, hpTextStyle);
        
        // Draw center text
        if (!string.IsNullOrEmpty(centerText))
        {
            GUIStyle bigStyle = new GUIStyle();
            bigStyle.fontSize = 50;
            bigStyle.normal.textColor = Color.white;
            bigStyle.alignment = TextAnchor.MiddleCenter;
            
            float yPos = Screen.height / 3;
            // Shadow
            GUI.Label(new Rect(Screen.width/2 - 202, yPos - 52, 400, 100), centerText, new GUIStyle(bigStyle) { normal = { textColor = Color.black } });
            // Text
            GUI.Label(new Rect(Screen.width/2 - 200, yPos - 50, 400, 100), centerText, bigStyle);
        }

        if (isGameOver)
        {
            // Game Over screen
            GUIStyle gameOverStyle = new GUIStyle();
            gameOverStyle.fontSize = 60;
            gameOverStyle.fontStyle = FontStyle.Bold;
            gameOverStyle.normal.textColor = Color.red;
            gameOverStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(Screen.width/2 - 200, Screen.height/2 - 100, 400, 80), "GAME OVER", gameOverStyle);
            
            // Restart button
            if (GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2, 120, 50), "Restart"))
            {
                Time.timeScale = 1f; // Unfreeze before restarting
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}