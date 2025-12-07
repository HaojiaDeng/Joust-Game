using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    private GameObject UI4;
    private GameObject UI20;
    private GameObject UI21;
    private GameObject UI22;
    private Text logText;
    private TextMeshProUGUI roundText;
    private TextMeshProUGUI logTextTMP;
    private Dictionary<GameObject, bool> uiActiveStates = new Dictionary<GameObject, bool>();
    
    private Canvas mainCanvas;
    private bool mainCanvasWasActive = true;
    private Camera gameCamera;
    private bool gameCameraWasEnabled = true;
    
    private static BattleUIManager _instance;
    public static BattleUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("BattleUIManager");
                _instance = go.AddComponent<BattleUIManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void Initialize(GameObject ui4, GameObject ui20, GameObject ui21, GameObject ui22, Text logTextRef, Canvas canvas, Camera camera)
    {
        UI4 = ui4;
        UI20 = ui20;
        UI21 = ui21;
        UI22 = ui22;
        logText = logTextRef;
        mainCanvas = canvas;
        gameCamera = camera;
        
        FindRoundText();
    }
    
    private void FindRoundText()
    {
        if (UI4 != null)
        {
            Transform roundTransform = UI4.transform.Find("Round");
            if (roundTransform == null)
            {
                roundTransform = FindChildByName(UI4.transform, "Round");
            }
            
            if (roundTransform != null)
            {
                roundText = roundTransform.GetComponent<TextMeshProUGUI>();
                
                if (roundText == null)
                {
                    Transform textTransform = roundTransform.Find("Text (TMP)");
                    if (textTransform == null)
                    {
                        textTransform = roundTransform.Find("Text");
                    }
                    if (textTransform != null)
                    {
                        roundText = textTransform.GetComponent<TextMeshProUGUI>();
                    }
                }
                
                if (roundText == null)
                {
                    roundText = roundTransform.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }
    }
    
    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    
    public void InitializeRoundDisplay()
    {
        if (roundText == null)
        {
            FindRoundText();
        }
        
        if (roundText != null)
        {
            if (roundText.text == "Round" || !roundText.text.StartsWith("Round "))
            {
                if (GameLoopManager.Instance != null)
                {
                    GameLoopManager.Instance.roundsCompleted = 1;
                }
                UpdateRoundDisplay(1);
            }
            else
            {
                if (GameLoopManager.Instance != null && roundText.text.StartsWith("Round "))
                {
                    string roundStr = roundText.text.Replace("Round ", "").Trim();
                    if (int.TryParse(roundStr, out int displayedRound))
                    {
                        GameLoopManager.Instance.roundsCompleted = displayedRound;
                    }
                }
            }
        }
    }
    
    public void IncrementRound()
    {
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.roundsCompleted++;
            UpdateRoundDisplay(GameLoopManager.Instance.roundsCompleted);
        }
    }
    
    private void UpdateRoundDisplay(int roundNumber)
    {
        if (roundText == null)
        {
            FindRoundText();
        }
        
        if (roundText != null)
        {
            roundText.text = "Round " + roundNumber.ToString();
        }
    }
    
    public void SetLogText(string text)
    {
        if (logText != null)
        {
            logText.text = text;
            return;
        }
        
        if (logTextTMP != null)
        {
            logTextTMP.text = text;
            return;
        }
        
        if (logText == null && logTextTMP == null)
        {
            string[] possibleNames = { "logtext", "logText", "LogText", "Card Left", "CardLeft" };
            foreach (string name in possibleNames)
            {
                GameObject logTextObj = GameObject.Find(name);
                if (logTextObj == null)
                {
                    if (UI4 != null)
                    {
                        Transform found = FindChildByName(UI4.transform, name);
                        if (found != null)
                        {
                            logTextObj = found.gameObject;
                        }
                    }
                }
                
                if (logTextObj != null)
                {
                    logText = logTextObj.GetComponent<Text>();
                    if (logText != null)
                    {
                        logText.text = text;
                        return;
                    }
                    
                    logTextTMP = logTextObj.GetComponent<TextMeshProUGUI>();
                    if (logTextTMP != null)
                    {
                        logTextTMP.text = text;
                        return;
                    }
                }
            }
            
            Text[] allTexts = FindObjectsByType<Text>(FindObjectsSortMode.None);
            foreach (Text txt in allTexts)
            {
                if (txt.text != null && txt.text.Contains("Card Left"))
                {
                    logText = txt;
                    logText.text = text;
                    return;
                }
            }
            
            TextMeshProUGUI[] allTMPs = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (TextMeshProUGUI tmp in allTMPs)
            {
                if (tmp.text != null && tmp.text.Contains("Card Left"))
                {
                    logTextTMP = tmp;
                    logTextTMP.text = text;
                    return;
                }
            }
        }
    }
    
    public void HideUIForQTE()
    {
        uiActiveStates.Clear();
        
        GameObject bgObj = GameObject.Find("Bg");
        if (bgObj != null)
        {
            UnityEngine.UI.Image bgImage = bgObj.GetComponent<UnityEngine.UI.Image>();
            if (bgImage != null)
            {
                if (!uiActiveStates.ContainsKey(bgObj))
                {
                    Color originalColor = bgImage.color;
                    bgImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
                }
            }
        }
        
        GameObject[] uiObjects = { UI4, UI20, UI21, UI22 };
        foreach (var ui in uiObjects)
        {
            if (ui == null) continue;
            uiActiveStates[ui] = ui.activeSelf;
            ui.SetActive(false);
        }
        
        if (gameCamera != null)
        {
            gameCameraWasEnabled = gameCamera.enabled;
        }
    }
    
    public void RestoreUIAfterQTE()
    {
        if (PlayerHealthUI.Instance != null && GameLoopManager.Instance != null)
        {
            PlayerHealthUI.Instance.UpdateHealth(GameLoopManager.Instance.currentHealth);
        }
        
        GameObject bgObj = GameObject.Find("Bg");
        if (bgObj != null)
        {
            bool wasActive;
            if (uiActiveStates.TryGetValue(bgObj, out wasActive))
            {
                bgObj.SetActive(wasActive);
            }
        }
        
        GameObject[] uiObjects = { UI4, UI20, UI21, UI22 };
        foreach (var ui in uiObjects)
        {
            if (ui == null) continue;
            
            bool wasActive;
            if (uiActiveStates.TryGetValue(ui, out wasActive))
            {
                ui.SetActive(wasActive);
            }
            else
            {
                ui.SetActive(true);
                if (ui == UI4)
                {
                    InitializeRoundDisplay();
                }
            }
        }
        
        if (mainCanvas != null)
        {
            mainCanvas.gameObject.SetActive(mainCanvasWasActive);
        }
        
        if (gameCamera != null)
        {
            gameCamera.enabled = gameCameraWasEnabled;
        }
    }
}
