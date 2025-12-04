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

    private void Start()
    {
        if (PlayerHealthUI.Instance != null)
        {
            PlayerHealthUI.Instance.UpdateHealth(currentHealth);
        }
    }

    public void TakeDamage()
    {
        if (isGameOver) return;
        currentHealth--;
        Debug.Log($"Damage taken. HP: {currentHealth}");
        
        if (PlayerHealthUI.Instance != null)
        {
            PlayerHealthUI.Instance.UpdateHealth(currentHealth);
        }
        
        if (currentHealth <= 0)
        {
            isGameOver = true;
            Time.timeScale = 0f; // Freeze game
            Debug.Log("Game Over - Game Frozen");
        }
    }

    public void Heal()
    {
        if (!isGameOver && currentHealth < maxHealth)
        {
            currentHealth++;
            roundsCompleted++;
            Debug.Log($"Healed. HP: {currentHealth}, Round: {roundsCompleted}");
            
            if (PlayerHealthUI.Instance != null)
            {
                PlayerHealthUI.Instance.UpdateHealth(currentHealth);
            }
        }
        else if (!isGameOver)
        {
            roundsCompleted++;
            Debug.Log($"Round: {roundsCompleted}, HP already full");
        }
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
        GUI.Label(new Rect(10, 10, 200, 20), $"HP: {currentHealth}/{maxHealth}");
        
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