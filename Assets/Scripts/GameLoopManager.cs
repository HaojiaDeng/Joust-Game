using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance;
    public int maxHealth = 10;
    public int currentHealth;
    public bool isGameOver = false;
    public bool isVictory = false; // Set to true when Boss is defeated
    public int roundsCompleted = 0;

    [Header("UI State")]
    public string centerText = "";
    public float centerTextTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        currentHealth = maxHealth;
        Debug.Log($"GameLoopManager: Initialized with {currentHealth}/{maxHealth} health");
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
        Debug.Log($"GameLoopManager: Damage taken. HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth < 0) currentHealth = 0;
        
        if (PlayerHealthUI.Instance != null)
        {
            PlayerHealthUI.Instance.UpdateHealth(currentHealth);
        }
        else
        {
            Debug.LogWarning("GameLoopManager: PlayerHealthUI.Instance is NULL! Searching for PlayerHealthUI component...");
            PlayerHealthUI ui = FindFirstObjectByType<PlayerHealthUI>();
            if (ui != null)
            {
                ui.UpdateHealth(currentHealth);
            }
            else
            {
                Debug.LogError("GameLoopManager: PlayerHealthUI component not found in scene!");
            }
        }
        
        if (currentHealth <= 0)
        {
            isGameOver = true;
            Time.timeScale = 0f;
            Debug.Log("Game Over - Player health reached 0. Game Frozen");
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
        if (centerTextTimer > 0)
        {
            centerTextTimer -= Time.unscaledDeltaTime;
            if (centerTextTimer <= 0) 
            {
                centerText = "";
                Debug.Log("Center text timer expired, text cleared");
            }
        }
    }

    private void OnGUI()
    {
        if (!string.IsNullOrEmpty(centerText))
        {
            GUIStyle bigStyle = new GUIStyle();
            bigStyle.fontSize = 50;
            bigStyle.normal.textColor = Color.white;
            bigStyle.alignment = TextAnchor.MiddleCenter;
            
            float yPos = Screen.height / 3;
            GUI.Label(new Rect(Screen.width/2 - 202, yPos - 52, 400, 100), centerText, new GUIStyle(bigStyle) { normal = { textColor = Color.black } });
            GUI.Label(new Rect(Screen.width/2 - 200, yPos - 50, 400, 100), centerText, bigStyle);
        }

        if (string.IsNullOrEmpty(centerText))
        {
            if (isVictory)
            {
                GUIStyle victoryStyle = new GUIStyle();
                victoryStyle.fontSize = 60;
                victoryStyle.fontStyle = FontStyle.Bold;
                victoryStyle.normal.textColor = Color.green;
                victoryStyle.alignment = TextAnchor.MiddleCenter;
                
                GUI.Label(new Rect(Screen.width/2 - 200, Screen.height/2 - 100, 400, 80), "YOU WIN!", victoryStyle);
                
                if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 50), "Back to Main Menu"))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("StartGame");
                }
            }
            else if (isGameOver)
            {
                GUIStyle gameOverStyle = new GUIStyle();
                gameOverStyle.fontSize = 60;
                gameOverStyle.fontStyle = FontStyle.Bold;
                gameOverStyle.normal.textColor = Color.red;
                gameOverStyle.alignment = TextAnchor.MiddleCenter;
                
                GUI.Label(new Rect(Screen.width/2 - 200, Screen.height/2 - 100, 400, 80), "GAME OVER", gameOverStyle);
                
                if (GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2, 120, 50), "Restart"))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
}