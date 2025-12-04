using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public GameObject clickEffect;
    public RectTransform attackParent;
    public Text logText;
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

    public int NumberBattleCards = 0;

    private Dictionary<GameObject, bool> uiActiveStates = new Dictionary<GameObject, bool>();

    // Main UI canvas and camera for the card scene (disabled during QTE)
    public Canvas mainCanvas;
    private bool mainCanvasWasActive = true;

    public Camera gameCamera;
    private bool gameCameraWasEnabled = true;

    private void Awake()
    {
        Instance = this;
        logText.text = "Card Left:" + logInt.ToString();

        // Make sure QTE hint text starts hidden
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
            UI20.SetActive(false);
            UI21.SetActive(false);
            UI22.SetActive(false);
            

        }
        
    }

    public void ChooseACard(string name)
    {
        vlaue++;
        GameObject cardObj = null;
        CardStats.CardType cardType = CardStats.CardType.Sword;
        
        switch (name)
        {
            case "Axe":
                cardObj = Instantiate(AXE, HandsPlayed);
                cardType = CardStats.CardType.Axe;
                break;
            case "Bow":
                cardObj = Instantiate(Bow, HandsPlayed);
                cardType = CardStats.CardType.Bow;
                break;
            case "Lance":
                cardObj = Instantiate(Lance, HandsPlayed);
                cardType = CardStats.CardType.Lance;
                break;
            case "Shield":
                cardObj = Instantiate(Shield, HandsPlayed);
                cardType = CardStats.CardType.Shield;
                break;
            case "Sword":
                cardObj = Instantiate(Sword, HandsPlayed);
                cardType = CardStats.CardType.Sword;
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

        GameObject[] uiObjects = { UI4, UI20, UI21, UI22 };
        foreach (var ui in uiObjects)
        {
            if (ui == null) continue;
            uiActiveStates[ui] = ui.activeSelf;
            ui.SetActive(false);
        }

        if (mainCanvas != null)
        {
            mainCanvasWasActive = mainCanvas.gameObject.activeSelf;
            mainCanvas.gameObject.SetActive(false);
        }

        if (gameCamera != null)
        {
            gameCameraWasEnabled = gameCamera.enabled;
            gameCamera.enabled = false;
        }
    }

    public void OnQTEFinished()
    {
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

        ReplenishHand();
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
        Debug.Log($"ReplenishHand: Hand size is now {cards.Count}/3, Deck size: {cardPositions.Count}");
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
            Debug.LogWarning("RefreshCardPositions: No source list available!");
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
        
        Debug.Log($"RefreshCardPositions: Source list has {sourceList.Count} cards, {cardsInHandSet.Count} in hand, {availableCount} available in deck");
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
        logText.text = "Card Left:" + logInt.ToString();
        if (cards.Contains(PlayACardObject))
        {
            cards.Remove(PlayACardObject);
        } 
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
            
            ShowBossCard();
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.LogWarning("BossStats.Instance is NULL!");
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

        StartCoroutine(LoadQTESceneAndSetup());
    }
    
    private IEnumerator LoadQTESceneAndSetup()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Additive);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        Scene qteScene = SceneManager.GetSceneByName("SampleScene");
        if (qteScene.IsValid() && qteScene.isLoaded)
        {
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            EventSystem qteEventSystem = null;
            
            foreach (var es in eventSystems)
            {
                if (es != null && es.gameObject.scene.name == "SampleScene")
                {
                    qteEventSystem = es;
                    break;
                }
            }
            
            if (qteEventSystem != null)
            {
                qteEventSystem.gameObject.SetActive(false);
                Debug.Log("Disabled EventSystem in QTE scene to prevent conflicts");
            }
            
            EnterQTE();
            
            if (BossBattle.Instance != null && BossBattle.Instance.IsBossBattleActive())
            {
                GameObject listenerObj = new GameObject("BossQTEListener");
                listenerObj.AddComponent<BossQTEListener>();
                SceneManager.MoveGameObjectToScene(listenerObj, qteScene);
                Debug.Log("BossQTEListener created in QTE scene");
            }
        }
    }

    private void ShowBossCard()
    {
        if (BossManager.Instance == null)
        {
            Debug.LogError("BossManager not found!");
            return;
        }

        CardStats.CardData bossCard = BossManager.Instance.SelectBossCard();
        if (bossCard == null)
        {
            Debug.LogError("Boss card selection failed!");
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
            Debug.LogError($"Card prefab not found for type: {bossCard.cardType}");
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

        Debug.Log($"Boss played card: {bossCard.cardType} at position: {bossCardOffset}");
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
}
