using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardManagement : MonoBehaviour
{
    public static CardManagement Instance;
    public GameObject AXE;
    public GameObject Bow;
    public GameObject Lance;
    public GameObject Shield;
    public GameObject Sword;
    public Transform HandsPlayed;

    public GameObject UI4;
    public GameObject UI20;
    public GameObject UI21;
    public GameObject UI22;
    public GameObject qteReadyText;
    
    private TextMeshProUGUI roundText;
    public GameObject clickEffect;
    public RectTransform attackParent;
    public Text logText;
    private TextMeshProUGUI logTextTMP;
    public Vector3 bossCardOffset = new Vector3(0f, 250f, 0f);
    public List<RectTransform> cardPositionsAttack = new List<RectTransform>();
    public List<RectTransform> cardPositionsBALANCE = new List<RectTransform>();
    public List<RectTransform> cardPositionsDEFENSE = new List<RectTransform>();
    public List<RectTransform> cardPositions = new List<RectTransform>();
    private List<RectTransform> originalCardPositions = new List<RectTransform>();
    private int vlaue = 0;
    private int logInt = 12;
    public List<GameObject> cards = new List<GameObject>();
    private GameObject PlayACardObject;
    private GameObject bossCardObject;
    private CardData currentPlayerCardData;

    public int NumberBattleCards = 0;

    private Dictionary<GameObject, bool> uiActiveStates = new Dictionary<GameObject, bool>();

    public Canvas mainCanvas;
    private bool mainCanvasWasActive = true;

    public Camera gameCamera;
    private bool gameCameraWasEnabled = true;

    [Header("QTE UI")]
    public GameObject qteUI;
    private bool qteUIWasActive = false;

    private void Awake()
    {
        Instance = this;
        FindRoundText();
        
        if (qteReadyText != null)
        {
            qteReadyText.SetActive(false);
        }

        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
            if (mainCanvas == null)
            {
                mainCanvas = FindFirstObjectByType<Canvas>();
            }
        }

        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        if (qteUI == null)
        {
            qteUI = GameObject.Find("QTE");
        }

        if (qteUI != null)
        {
            qteUIWasActive = qteUI.activeSelf;
            qteUI.SetActive(false);
            
            RandomEncounterManager randomEncounter = qteUI.GetComponentInChildren<RandomEncounterManager>();
            if (randomEncounter != null)
            {
                randomEncounter.enabled = false;
            }
        }
    }
    
    private void Start()
    {
        StartCoroutine(InitializeLogTextDelayed());
    }
    
    private IEnumerator InitializeLogTextDelayed()
    {
        yield return null;
        
        int displayCount = Mathf.Max(0, logInt - 3);
        string textToDisplay = "Card Left: " + displayCount.ToString();
        SetLogText(textToDisplay);
    }

    public void StartGame(string cardName)
    {
        List<RectTransform> sourceList = null;
        switch (cardName)
        {
            case "Attack":
                sourceList = cardPositionsAttack;
                break;
            case "BALANCE":
                sourceList = cardPositionsBALANCE;
                break;
            case "DEFENSE":
                sourceList = cardPositionsDEFENSE;
                break;
        }

        if (sourceList != null)
        {
            originalCardPositions = new List<RectTransform>(sourceList);
            cardPositions = new List<RectTransform>(sourceList);
            CardAnimation.Instance.StartCardMoveAnimation(sourceList);
        }
    }

    public void AddCard(GameObject cardObj)
    {
        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        if (cardPositions.Contains(rectTransform))
        {
            cardPositions.Remove(rectTransform);
        }

        Button buttonsword = cardObj.GetComponent<Button>();
        buttonsword.onClick.RemoveAllListeners();
        buttonsword.onClick.AddListener(() => PlayACard(cardObj));
        cardObj.transform.SetParent(attackParent);
        cards.Add(cardObj);
        switch (NumberBattleCards)
        {
            case 0:
                cards[NumberBattleCards].transform.localPosition = new Vector3(300f, -300f, 0f);
                break;
            case 1:
                cards[NumberBattleCards].transform.localPosition = new Vector3(0f, -300f, 0f);
                break;
            case 2:
                cards[NumberBattleCards].transform.localPosition = new Vector3(-300f, -300f, 0f);
                break;
        }      
        
        NumberBattleCards++;
        cardObj.SetActive(false);
        if (NumberBattleCards >= 3)
        {
            cards[0].SetActive(true);
            cards[1].SetActive(true);
            cards[2].SetActive(true);
            foreach(var rectran in cardPositions)
            {
                Button btn = rectran.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
            }
            UI4.SetActive(true);
            InitializeRoundDisplay();
            UI20.SetActive(false);
            UI21.SetActive(false);
            UI22.SetActive(false);
            

        }
        
    }

    public void ChooseACard(string name)
    {
        vlaue++;
        GameObject cardObj = null;
        
        switch (name)
        {
            case "Axe":
                cardObj = Instantiate(AXE, HandsPlayed);
                break;
            case "Bow":
                cardObj = Instantiate(Bow, HandsPlayed);
                break;
            case "Lance":
                cardObj = Instantiate(Lance, HandsPlayed);
                break;
            case "Shield":
                cardObj = Instantiate(Shield, HandsPlayed);
                break;
            case "Sword":
                cardObj = Instantiate(Sword, HandsPlayed);
                break;
            default:
            break;
        }
        
        if (cardObj != null)
        {
            Button button = cardObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => PlayACard(cardObj));
            }
            cards.Add(cardObj);
        }
        if(vlaue >= 3)
        {
            cards[0].transform.position = new Vector3(300f, -300f, 0f);
            cards[1].transform.position = new Vector3(0f, -300f, 0f);
            cards[2].transform.position = new Vector3(-300f, -300f, 0f);
            UI4.SetActive(true);
            InitializeRoundDisplay();
            UI20.SetActive(false);
            UI21.SetActive(false);
            UI22.SetActive(false);
        }
    }


    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GameObject newObj = Instantiate(clickEffect);
            Vector3 mousePos = Input.mousePosition;
            newObj.transform.SetParent(this.transform, false);
            newObj.transform.position = mousePos;
            Destroy(newObj, 0.5f);
        }
    }


    public void PlayACard(GameObject obj)
    {
        PlayACardObject = obj;
        PlayACardObject.SetActive(true);
        foreach (var card in cards)
        {
			Vector3 originalPos = card.transform.localPosition;
			card.transform.localPosition = new Vector3(originalPos.x, -300f, originalPos.z);

		}
        obj.transform.position += new Vector3(0, 40, 0);
        
    }

    public void EnterQTE()
    {
        CleanupBossCard();

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

    public void OnQTESuccess(CardData playerCard, QTEResult qteResult)
    {
        RestoreUIAndCamera();

        if (playerCard != null && BossStats.Instance != null && BossStats.Instance.IsActive())
        {
            int finalDamage = Mathf.RoundToInt(playerCard.baseDamage * qteResult.damageMultiplier);
            
            CardStats.CardData bossCard = BossBattle.Instance != null ? BossBattle.Instance.GetBossCard() : null;
            
            string advantageText = "";
            if (bossCard != null && WeaponAdvantageSystem.Instance != null)
            {
                WeaponCategory playerWeapon = playerCard.category;
                WeaponCategory bossWeapon = ConvertCardTypeToWeaponCategory(bossCard.cardType);
                
                float advantageMultiplier = WeaponAdvantageSystem.Instance.GetAdvantageMultiplier(playerWeapon, bossWeapon);
                advantageText = WeaponAdvantageSystem.Instance.GetAdvantageText(playerWeapon, bossWeapon);
            }
            
            StartCoroutine(ApplyBossDamageWithVisualFeedback(finalDamage, advantageText));
        }

        IncrementRound();

        ReplenishHand();
    }

    public void OnQTEFailed(CardData playerCard)
    {
        RestoreUIAndCamera();

        if (playerCard != null && GameLoopManager.Instance != null && BossBattle.Instance != null)
        {
            CardStats.CardData bossCard = BossBattle.Instance.GetBossCard();
            
            if (bossCard != null)
            {
                WeaponCategory bossWeapon = ConvertCardTypeToWeaponCategory(bossCard.cardType);
                WeaponCategory playerWeapon = playerCard.category;
                
                float advantageMultiplier = 1.0f;
                if (WeaponAdvantageSystem.Instance != null)
                {
                    advantageMultiplier = WeaponAdvantageSystem.Instance.GetAdvantageMultiplier(bossWeapon, playerWeapon);
                }
                
                int baseDamage = 2;
                int finalDamage = Mathf.RoundToInt(baseDamage * advantageMultiplier);
                if (finalDamage < 1) finalDamage = 1;
                
                StartCoroutine(ApplyPlayerDamageWithVisualFeedback(finalDamage));
                
                string advantageText = "";
                if (WeaponAdvantageSystem.Instance != null)
                {
                    advantageText = WeaponAdvantageSystem.Instance.GetAdvantageText(bossWeapon, playerWeapon);
                }
                
                string damageMessage = $"Player takes {finalDamage} damage!";
                if (!string.IsNullOrEmpty(advantageText))
                {
                    damageMessage += $" ({advantageText})";
                }
                
                GameLoopManager.Instance.ShowCenterMessage(damageMessage, 2f);
            }
            else
            {
                GameLoopManager.Instance.TakeDamage();
            }
        }

        IncrementRound();

        ReplenishHand();
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
                if (roundText != null)
                {
                    return;
                }
                
                string[] possibleChildNames = { "Text (TMP)", "Text", "TextMeshPro", "TextMeshProUGUI", "RoundText", "TextComponent" };
                foreach (string childName in possibleChildNames)
                {
                    Transform textTransform = roundTransform.Find(childName);
                    if (textTransform != null)
                    {
                        roundText = textTransform.GetComponent<TextMeshProUGUI>();
                        if (roundText != null)
                        {
                            return;
                        }
                    }
                }
                
                roundText = roundTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (roundText != null)
                {
                    return;
                }
                
                TextMeshProUGUI[] allTextComponents = roundTransform.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (allTextComponents != null && allTextComponents.Length > 0)
                {
                    roundText = allTextComponents[0];
                    return;
                }
                
                roundText = FindTextMeshProInChildren(roundTransform);
                if (roundText != null)
                {
                    return;
                }
            }
            else
            {
                TextMeshProUGUI[] allTexts = UI4.GetComponentsInChildren<TextMeshProUGUI>(true);
            }
        }
    }
    
    private TextMeshProUGUI FindTextMeshProInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                return tmp;
            
            TextMeshProUGUI found = FindTextMeshProInChildren(child);
            if (found != null)
                return found;
        }
        return null;
    }
    
    private string GetFullPath(Transform transform)
    {
        if (transform == null) return "";
        if (transform.parent == null) return transform.name;
        return GetFullPath(transform.parent) + "/" + transform.name;
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
    
    private void InitializeRoundDisplay()
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
    
    private void IncrementRound()
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
    
    private System.Collections.IEnumerator ApplyBossDamageWithVisualFeedback(int damage, string advantageText = "")
    {
        string damageMessage = $"Boss takes {damage} damage!";
        if (!string.IsNullOrEmpty(advantageText))
        {
            damageMessage += $" ({advantageText})";
        }
        
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.ShowCenterMessage(damageMessage, 2f);
        }
        
        for (int i = 0; i < damage; i++)
        {
            if (BossStats.Instance != null && BossStats.Instance.IsActive())
            {
                BossStats.Instance.currentHealth--;
                if (BossStats.Instance.currentHealth < 0)
                {
                    BossStats.Instance.currentHealth = 0;
                }
                
                if (BossHealthUI.Instance != null)
                {
                    BossHealthUI.Instance.UpdateHealth(BossStats.Instance.currentHealth);
                }
                
                if (BossStats.Instance.IsDefeated())
                {
                    BossStats.Instance.DefeatBoss();
                    if (GameLoopManager.Instance != null)
                    {
                        GameLoopManager.Instance.ShowCenterMessage("BOSS DEFEATED!", 3f);
                        GameLoopManager.Instance.isVictory = true;
                        Time.timeScale = 0f;
                    }
                    break;
                }
                
                if (i < damage - 1)
                {
                    yield return new WaitForSeconds(0.15f);
                }
            }
            else
            {
                break;
            }
        }
    }
    
    private System.Collections.IEnumerator ApplyPlayerDamageWithVisualFeedback(int damage)
    {
        for (int i = 0; i < damage; i++)
        {
            if (GameLoopManager.Instance != null && !GameLoopManager.Instance.isGameOver)
            {
                GameLoopManager.Instance.TakeDamage();
                
                if (i < damage - 1)
                {
                    yield return new WaitForSeconds(0.15f);
                }
            }
            else
            {
                break;
            }
        }
    }
    
    private WeaponCategory ConvertCardTypeToWeaponCategory(CardStats.CardType cardType)
    {
        switch (cardType)
        {
            case CardStats.CardType.Sword:
            case CardStats.CardType.Axe:
                return WeaponCategory.SwordAxe;
            case CardStats.CardType.Bow:
            case CardStats.CardType.Lance:
                return WeaponCategory.BowLance;
            case CardStats.CardType.Shield:
                return WeaponCategory.Shield;
            default:
                return WeaponCategory.SwordAxe;
        }
    }

    private void RestoreUIAndCamera()
    {
        if (qteUI != null)
        {
            qteUI.SetActive(qteUIWasActive);
        }
        
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

    private void ReplenishHand()
    {
        cards.RemoveAll(card => card == null);
        
        RefreshCardPositions();
        
        while (cards.Count < 3 && cardPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, cardPositions.Count);
            RectTransform randomCardRect = cardPositions[randomIndex];
            if (randomCardRect == null) continue;
            
            GameObject randomCardObj = randomCardRect.gameObject;
            if (randomCardObj == null) continue;
            
            Button buttonsword = randomCardObj.GetComponent<Button>();
            if (buttonsword != null)
            {
                buttonsword.onClick.RemoveAllListeners();
                buttonsword.onClick.AddListener(() => PlayACard(randomCardObj));
            }
            randomCardObj.transform.SetParent(attackParent);
            
            if (cardPositions.Contains(randomCardRect))
            {
                cardPositions.Remove(randomCardRect);
            }
            cards.Add(randomCardObj);
        }

        UpdateCardPositions();
    }

    private void RefreshCardPositions()
    {
        List<RectTransform> sourceList = null;
        
        if (originalCardPositions != null && originalCardPositions.Count > 0)
        {
            sourceList = originalCardPositions;
        }
        else
        {
            if (cardPositionsAttack.Count > 0)
            {
                sourceList = cardPositionsAttack;
            }
            else if (cardPositionsBALANCE.Count > 0)
            {
                sourceList = cardPositionsBALANCE;
            }
            else if (cardPositionsDEFENSE.Count > 0)
            {
                sourceList = cardPositionsDEFENSE;
            }
        }
        
        if (sourceList == null || sourceList.Count == 0)
        {
            return;
        }

        cardPositions.Clear();
        HashSet<GameObject> cardsInHandSet = new HashSet<GameObject>();
        foreach (GameObject card in cards)
        {
            if (card != null)
            {
                cardsInHandSet.Add(card);
            }
        }
        
        int availableCount = 0;
        foreach (RectTransform cardRect in sourceList)
        {
            if (cardRect == null) continue;
            
            GameObject cardObj = cardRect.gameObject;
            if (cardObj == null) continue;
            
            if (!cardsInHandSet.Contains(cardObj))
            {
                cardPositions.Add(cardRect);
                availableCount++;
            }
        }
    }

    private void UpdateCardPositions()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 pos;
            switch (i)
            {
                case 0:
                    pos = new Vector3(300f, -300f, 0f);
                    break;
                case 1:
                    pos = new Vector3(0f, -300f, 0f);
                    break;
                case 2:
                    pos = new Vector3(-300f, -300f, 0f);
                    break;
                default:
                    pos = new Vector3((i - 1) * -300f, -300f, 0f);
                    break;
            }
            cards[i].transform.localPosition = pos;
            cards[i].SetActive(true);
        }

        if (cards.Count >= 3)
        {
            foreach (var rectran in cardPositions)
            {
                Button btn = rectran.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
            }
            if (UI4 != null)
            {
                UI4.SetActive(true);
            InitializeRoundDisplay();
            }
            if (UI20 != null)
            {
                UI20.SetActive(false);
            }
            if (UI21 != null)
            {
                UI21.SetActive(false);
            }
            if (UI22 != null)
            {
                UI22.SetActive(false);
            }
        }
    }
    public void Play()
    {
        logInt--;
        int displayCount = Mathf.Max(0, logInt - 3);
        string textToDisplay = "Card Left: " + displayCount.ToString();
        SetLogText(textToDisplay);
        if (cards.Contains(PlayACardObject))
        {
            cards.Remove(PlayACardObject);
        } 
        
        currentPlayerCardData = CreateCardDataFromGameObject(PlayACardObject);
        
        StartCoroutine(FinishTheMove());
		
	}

    IEnumerator FinishTheMove()
    {
        Vector3 Pos = PlayACardObject.transform.position;

        CardAnimation.Instance.PlayCard(PlayACardObject.GetComponent<RectTransform>());
        yield return new WaitForSeconds(0.45f);
        
        RefreshCardPositions();
        
        if (cardPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, cardPositions.Count);
            RectTransform randomCardRect = cardPositions[randomIndex];
            GameObject randomCardObj = randomCardRect.gameObject;
            Button buttonsword = randomCardObj.GetComponent<Button>();
            buttonsword.onClick.AddListener(() => PlayACard(randomCardObj));
            randomCardObj.transform.SetParent(attackParent);
            randomCardObj.transform.position = Pos;
            randomCardObj.SetActive(true);
            if (cardPositions.Contains(randomCardRect))
            {
                cardPositions.Remove(randomCardRect);
            }
            cards.Add(randomCardObj);
            
			foreach (var card in cards)
			{
				Vector3 originalPos = card.transform.localPosition;
				card.transform.localPosition = new Vector3(originalPos.x, -300f, originalPos.z);

			}
		}
        
        GameObject cardToDestroy = PlayACardObject;
        PlayACardObject = null;
        
        Destroy(cardToDestroy);

        if (BossStats.Instance != null)
        {
            if (!BossStats.Instance.IsActive())
            {
                BossManager.Instance?.StartBossBattle();
            }
            
            if (BossBattle.Instance != null)
            {
                BossBattle.Instance.StartBossBattle();
            }
            
            InitializeRoundDisplay();
            
            ShowBossCard();
            yield return new WaitForSeconds(1f);
        }

        if (qteReadyText != null)
        {
            qteReadyText.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (qteReadyText != null)
        {
            qteReadyText.SetActive(false);
        }

        StartCoroutine(StartQTESetup());
    }
    
    private IEnumerator StartQTESetup()
    {
        if (qteUI == null)
        {
            qteUI = GameObject.Find("QTE");
            if (qteUI == null)
            {
                yield break;
            }
        }

        RandomEncounterManager randomEncounter = qteUI.GetComponentInChildren<RandomEncounterManager>();
        if (randomEncounter != null)
        {
            randomEncounter.enabled = false;
        }

        EnterQTE();
        
        yield return new WaitForSeconds(0.1f);
        
        qteUIWasActive = qteUI.activeSelf;
        qteUI.SetActive(true);
        
        Transform playerTransform = qteUI.transform.Find("Player");
        Transform enemyTransform = qteUI.transform.Find("Enemy");
        
        if (playerTransform != null)
        {
            playerTransform.gameObject.SetActive(true);
        }
        
        if (enemyTransform != null)
        {
            enemyTransform.gameObject.SetActive(true);
        }
        
        Canvas canvas = qteUI.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GameObject bgObj = GameObject.Find("Bg");
            if (bgObj != null)
            {
                Canvas bgCanvas = bgObj.GetComponentInParent<Canvas>();
                if (bgCanvas != null && bgCanvas != canvas)
                {
                    if (canvas.sortingOrder <= bgCanvas.sortingOrder)
                    {
                        canvas.sortingOrder = bgCanvas.sortingOrder + 10;
                    }
                }
            }
        }
        
        yield return new WaitForEndOfFrame();
        
        QTEUIController uiController = qteUI.GetComponentInChildren<QTEUIController>(true);
        if (uiController != null)
        {
            uiController.HidePrompt();
        }
        
        GameObject promptPanel = GameObject.Find("PromptPanel");
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
        
        GameObject promptTextObj = GameObject.Find("PromptText");
        if (promptTextObj != null)
        {
            TMPro.TextMeshProUGUI promptText = promptTextObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (promptText != null)
            {
                promptText.text = "";
            }
        }
        
        UnityEngine.UI.RawImage[] rawImages = qteUI.GetComponentsInChildren<UnityEngine.UI.RawImage>(true);
        foreach (var rawImage in rawImages)
        {
            if (rawImage.texture != null && rawImage.texture is RenderTexture)
            {
                rawImage.gameObject.SetActive(false);
            }
        }
        
        Transform[] allChildren = qteUI.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name.Contains("Display", System.StringComparison.OrdinalIgnoreCase) || 
                child.name.Contains("Camera", System.StringComparison.OrdinalIgnoreCase))
            {
                child.gameObject.SetActive(false);
            }
        }
        
        if (qteReadyText != null)
        {
            UnityEngine.UI.RawImage readyTextRawImage = qteReadyText.GetComponent<UnityEngine.UI.RawImage>();
            if (readyTextRawImage != null)
            {
                if (readyTextRawImage.texture is RenderTexture)
                {
                    qteReadyText.SetActive(false);
                }
            }
            if (qteReadyText.name.Contains("Display", System.StringComparison.OrdinalIgnoreCase))
            {
                qteReadyText.SetActive(false);
            }
        }
            
        if (BossBattle.Instance != null && BossBattle.Instance.IsBossBattleActive())
        {
            JoustController joustController = qteUI.GetComponentInChildren<JoustController>(true);
            if (joustController == null)
            {
                joustController = FindFirstObjectByType<JoustController>();
            }
            
            if (joustController != null && currentPlayerCardData != null)
            {
                joustController.StartJoust(currentPlayerCardData);
            }
        }
    }

    private void ShowBossCard()
    {
        if (BossManager.Instance == null)
        {
            return;
        }

        CardStats.CardData bossCard = BossManager.Instance.SelectBossCard();
        if (bossCard == null)
        {
            return;
        }

        if (bossCardObject != null)
        {
            Destroy(bossCardObject);
            bossCardObject = null;
        }

        GameObject cardPrefab = GetCardPrefabByType(bossCard.cardType);
        if (cardPrefab == null)
        {
            return;
        }

        bossCardObject = Instantiate(cardPrefab, attackParent);
        RectTransform bossCardRect = bossCardObject.GetComponent<RectTransform>();
        if (bossCardRect != null)
        {
            bossCardRect.localPosition = bossCardOffset;
        }
        else
        {
            bossCardObject.transform.localPosition = bossCardOffset;
        }

        Button bossButton = bossCardObject.GetComponent<Button>();
        if (bossButton != null)
        {
            bossButton.enabled = false;
        }
    }

    private GameObject GetCardPrefabByType(CardStats.CardType cardType)
    {
        switch (cardType)
        {
            case CardStats.CardType.Axe:
                return AXE;
            case CardStats.CardType.Lance:
                return Lance;
            case CardStats.CardType.Bow:
                return Bow;
            case CardStats.CardType.Sword:
                return Sword;
            case CardStats.CardType.Shield:
                return Shield;
            default:
                return null;
        }
    }


    private void CleanupBossCard()
    {
        if (bossCardObject != null)
        {
            Destroy(bossCardObject);
            bossCardObject = null;
        }
    }

    private CardData CreateCardDataFromGameObject(GameObject cardObj)
    {
        if (cardObj == null) return null;

        string cardName = cardObj.name;
        if (cardName.Contains("(Clone)"))
        {
            cardName = cardName.Replace("(Clone)", "").Trim();
        }

        CardStats.CardType cardType = GetCardTypeFromName(cardName);
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.cardName = cardType.ToString();
        cardData.baseDamage = 2;
        cardData.minDamage = 1;
        cardData.maxDamage = 3;

        CardStats.CardData stats = CardStats.GetCardStats(cardType);
        if (stats != null)
        {
            switch (cardType)
            {
                case CardStats.CardType.Sword:
                case CardStats.CardType.Axe:
                    cardData.category = WeaponCategory.SwordAxe;
                    break;
                case CardStats.CardType.Bow:
                case CardStats.CardType.Lance:
                    cardData.category = WeaponCategory.BowLance;
                    break;
                case CardStats.CardType.Shield:
                    cardData.category = WeaponCategory.Shield;
                    break;
            }
        }

        QTEPattern pattern = FindFirstObjectByType<RandomEncounterManager>() != null
            ? GenerateRandomQTEPattern()
            : CreateDefaultQTEPattern();
        cardData.qtePattern = pattern;

        return cardData;
    }

    private CardStats.CardType GetCardTypeFromName(string name)
    {
        string lowerName = name.ToLower();
        if (lowerName.Contains("axe")) return CardStats.CardType.Axe;
        if (lowerName.Contains("bow")) return CardStats.CardType.Bow;
        if (lowerName.Contains("lance")) return CardStats.CardType.Lance;
        if (lowerName.Contains("shield")) return CardStats.CardType.Shield;
        if (lowerName.Contains("sword")) return CardStats.CardType.Sword;
        return CardStats.CardType.Sword;
    }

    private QTEPattern GenerateRandomQTEPattern()
    {
        if (FindFirstObjectByType<RandomEncounterManager>() == null || GameLoopManager.Instance == null)
        {
            return CreateDefaultQTEPattern();
        }

        QTEType[] qteTypes = {
            QTEType.Sequence,
            QTEType.Directional,
            QTEType.Rhythm,
            QTEType.ButtonMash,
            QTEType.HoldAndRelease
        };

        QTEType selectedType = qteTypes[Random.Range(0, qteTypes.Length)];
        QTEPattern pattern = ScriptableObject.CreateInstance<QTEPattern>();
        pattern.qteType = selectedType;
        pattern.gapBetweenInputs = 0.5f;
        pattern.mustBeConsecutive = false;
        pattern.perfectBonus = 1.5f;
        pattern.failurePenalty = 0.5f;

        int keyCount = 3;
        pattern.inputSequence = new List<QTEInput>();
        for (int i = 0; i < keyCount; i++)
        {
            pattern.inputSequence.Add(new QTEInput
            {
                requiredKey = KeyCode.Space,
                windowDuration = 1.0f,
                delayBeforeThisPrompt = i == 0 ? 0.5f : 0f
            });
        }
        pattern.minimumSuccessfulInputs = Mathf.Max(1, keyCount - 1);

        return pattern;
    }

    private QTEPattern CreateDefaultQTEPattern()
    {
        QTEPattern pattern = ScriptableObject.CreateInstance<QTEPattern>();
        pattern.qteType = QTEType.Sequence;
        pattern.gapBetweenInputs = 0.5f;
        pattern.inputSequence = new List<QTEInput>
        {
            new QTEInput { requiredKey = KeyCode.Space, windowDuration = 1.0f, delayBeforeThisPrompt = 0.5f }
        };
        pattern.minimumSuccessfulInputs = 1;
        pattern.perfectBonus = 1.5f;
        pattern.failurePenalty = 0.5f;
        return pattern;
    }
    
    private void SetLogText(string text)
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
}

