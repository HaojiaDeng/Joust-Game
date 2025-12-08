using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardHandManager
{
    private List<GameObject> cards;
    private List<RectTransform> cardPositions;
    private List<RectTransform> originalCardPositions;
    
    private RectTransform attackParent;
    private List<RectTransform> cardPositionsAttack;
    private List<RectTransform> cardPositionsBALANCE;
    private List<RectTransform> cardPositionsDEFENSE;
    private System.Action<GameObject> onCardPlayed;
    
    public void Initialize(
        RectTransform parent,
        List<RectTransform> attack,
        List<RectTransform> balance,
        List<RectTransform> defense,
        System.Action<GameObject> cardPlayedCallback,
        List<GameObject> cardsList,
        List<RectTransform> cardPositionsList,
        List<RectTransform> originalCardPositionsList)
    {
        attackParent = parent;
        cardPositionsAttack = attack;
        cardPositionsBALANCE = balance;
        cardPositionsDEFENSE = defense;
        onCardPlayed = cardPlayedCallback;
        cards = cardsList;
        cardPositions = cardPositionsList;
        originalCardPositions = originalCardPositionsList;
    }
    
    public List<GameObject> GetCards() => cards;
    
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

        Button button = cardObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onCardPlayed?.Invoke(cardObj));
        }
        
        cardObj.transform.SetParent(attackParent);
        cards.Add(cardObj);
        
        UpdateCardPosition(cardObj, cards.Count - 1);
        cardObj.SetActive(false);
    }
    
    public void RemoveCard(GameObject cardObj)
    {
        if (cardObj == null) return;
        
        if (cards.Contains(cardObj))
        {
            cards.Remove(cardObj);
        }
    }
    
    public void RefreshCardPositions()
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
        int handSize = Mathf.Min(cards.Count, 3);
        for (int i = 0; i < handSize; i++)
        {
            if (cards[i] != null)
            {
                cardsInHandSet.Add(cards[i]);
            }
        }
        
        foreach (RectTransform cardRect in sourceList)
        {
            if (cardRect == null) continue;
            
            GameObject cardObj = cardRect.gameObject;
            if (cardObj == null) continue;
            
            if (!cardsInHandSet.Contains(cardObj))
            {
                cardObj.SetActive(false);
                cardPositions.Add(cardRect);
            }
        }
    }
    
    public void UpdateCardPositions()
    {
        HashSet<GameObject> uniqueCardsSet = new HashSet<GameObject>();
        List<GameObject> cleanCardsList = new List<GameObject>();
        foreach (GameObject card in cards)
        {
            if (card != null && !uniqueCardsSet.Contains(card))
            {
                uniqueCardsSet.Add(card);
                cleanCardsList.Add(card);
            }
        }
        
        while (cleanCardsList.Count > 3)
        {
            GameObject extraCard = cleanCardsList[cleanCardsList.Count - 1];
            cleanCardsList.RemoveAt(cleanCardsList.Count - 1);
            
            RectTransform cardRect = extraCard.GetComponent<RectTransform>();
            if (cardRect != null && !cardPositions.Contains(cardRect))
            {
                cardPositions.Add(cardRect);
            }
            extraCard.SetActive(false);
        }
        cards = cleanCardsList;
        
        HashSet<GameObject> cardsInHandSet = new HashSet<GameObject>();
        int handSize = Mathf.Min(cards.Count, 3);
        for (int i = 0; i < handSize; i++)
        {
            if (cards[i] != null)
            {
                cardsInHandSet.Add(cards[i]);
            }
        }
        
        List<RectTransform> allCardLists = new List<RectTransform>();
        if (originalCardPositions != null)
        {
            allCardLists.AddRange(originalCardPositions);
        }
        if (cardPositionsAttack != null)
        {
            allCardLists.AddRange(cardPositionsAttack);
        }
        if (cardPositionsBALANCE != null)
        {
            allCardLists.AddRange(cardPositionsBALANCE);
        }
        if (cardPositionsDEFENSE != null)
        {
            allCardLists.AddRange(cardPositionsDEFENSE);
        }
        if (cardPositions != null)
        {
            allCardLists.AddRange(cardPositions);
        }
        
        foreach (var rect in allCardLists)
        {
            if (rect == null) continue;
            GameObject cardObj = rect.gameObject;
            if (cardObj != null && !cardsInHandSet.Contains(cardObj))
            {
                cardObj.SetActive(false);
            }
        }
        
        for (int i = 3; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].SetActive(false);
            }
        }
        
        for (int i = 0; i < handSize; i++)
        {
            if (cards[i] == null) continue;
            
            UpdateCardPosition(cards[i], i);
            cards[i].SetActive(true);
        }

        if (cards.Count >= 3)
        {
            foreach (var rectran in cardPositions)
            {
                if (rectran == null) continue;
                Button btn = rectran.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                }
            }
        }
    }
    
    private void UpdateCardPosition(GameObject card, int index)
    {
        if (card == null) return;
        
        if (card.transform == null) return;
        
        Vector3 pos;
        switch (index)
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
                pos = new Vector3((index - 1) * -300f, -300f, 0f);
                break;
        }
        
        try
        {
            card.transform.localPosition = pos;
        }
        catch (System.Exception)
        {
        }
    }
    
    public void ReplenishHand()
    {
        HashSet<GameObject> uniqueCards = new HashSet<GameObject>();
        List<GameObject> cleanCards = new List<GameObject>();
        foreach (GameObject card in cards)
        {
            if (card != null && !uniqueCards.Contains(card))
            {
                uniqueCards.Add(card);
                cleanCards.Add(card);
            }
        }
        cards = cleanCards;
        
        RefreshCardPositions();
        
        if (cards.Count >= 3)
        {
            HideAllCardsNotInHand();
            UpdateCardPositions();
            return;
        }
        
        while (cards.Count > 3)
        {
            GameObject extraCard = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            
            RectTransform cardRect = extraCard.GetComponent<RectTransform>();
            if (cardRect != null && !cardPositions.Contains(cardRect))
            {
                cardPositions.Add(cardRect);
            }
            extraCard.SetActive(false);
        }
        
        while (cards.Count < 3 && cardPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, cardPositions.Count);
            RectTransform randomCardRect = cardPositions[randomIndex];
            if (randomCardRect == null) continue;
            
            GameObject randomCardObj = randomCardRect.gameObject;
            if (randomCardObj == null) continue;
            
            if (cards.Contains(randomCardObj))
            {
                cardPositions.Remove(randomCardRect);
                continue;
            }
            
            Button button = randomCardObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onCardPlayed?.Invoke(randomCardObj));
            }
            randomCardObj.transform.SetParent(attackParent);
            
            if (cardPositions.Contains(randomCardRect))
            {
                cardPositions.Remove(randomCardRect);
            }
            cards.Add(randomCardObj);
        }

        HideAllCardsNotInHand();

        UpdateCardPositions();
    }
    
    public void MoveAllCardsDown()
    {
        foreach (var card in cards)
        {
            if (card == null) continue;
            Vector3 originalPos = card.transform.localPosition;
            card.transform.localPosition = new Vector3(originalPos.x, -300f, originalPos.z);
        }
    }
    
    public void ReplenishOneCardAfterPlay()
    {
        cards.RemoveAll(card => card == null);
        
        RefreshCardPositions();
        
        if (cards.Count < 3)
        {
            List<RectTransform> availableCards = new List<RectTransform>();
            foreach (RectTransform cardRect in cardPositions)
            {
                if (cardRect != null && cardRect.gameObject != null)
                {
                    GameObject cardObj = cardRect.gameObject;
                    if (cardObj != null && !cards.Contains(cardObj))
                    {
                        availableCards.Add(cardRect);
                    }
                }
            }
            
            if (availableCards.Count > 0)
            {
                int attempts = 0;
                while (cards.Count < 3 && availableCards.Count > 0 && attempts < 10)
                {
                    int randomIndex = Random.Range(0, availableCards.Count);
                    RectTransform selectedCardRect = availableCards[randomIndex];
                    if (selectedCardRect != null && selectedCardRect.gameObject != null)
                    {
                        GameObject selectedCardObj = selectedCardRect.gameObject;
                        if (selectedCardObj != null && !cards.Contains(selectedCardObj))
                        {
                            AddCard(selectedCardObj);
                            availableCards.RemoveAt(randomIndex);
                            break;
                        }
                        else
                        {
                            availableCards.RemoveAt(randomIndex);
                        }
                    }
                    else
                    {
                        availableCards.RemoveAt(randomIndex);
                    }
                    attempts++;
                }
            }
        }
        
        MoveAllCardsDown();
        
        UpdateCardPositions();
    }
    
    public void ClearHand()
    {
        cards.Clear();
        cardPositions.Clear();
        originalCardPositions.Clear();
    }
    
    public void ShowHiddenHand()
    {
        cards.RemoveAll(card => card == null);
        
        HashSet<GameObject> cardsInHandSet = new HashSet<GameObject>();
        int handSize = Mathf.Min(cards.Count, 3);
        for (int i = 0; i < handSize; i++)
        {
            if (cards[i] != null)
            {
                cardsInHandSet.Add(cards[i]);
            }
        }
        
        RefreshCardPositions();
        
        HideAllCardsNotInHand();
        
        for (int i = 0; i < handSize; i++)
        {
            if (cards[i] == null)
            {
                continue;
            }
            
            try
            {
                string cardName = cards[i].name;
                
                UpdateCardPosition(cards[i], i);
                cards[i].SetActive(true);
            }
            catch (System.Exception)
            {
                cards[i] = null;
            }
        }
        
        for (int i = 3; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].SetActive(false);
            }
        }
    }
    
    private void HideAllCardsNotInHand()
    {
        HashSet<GameObject> cardsInHandSet = new HashSet<GameObject>();
        int handSize = Mathf.Min(cards.Count, 3);
        for (int i = 0; i < handSize; i++)
        {
            if (cards[i] != null)
            {
                cardsInHandSet.Add(cards[i]);
            }
        }
        
        List<RectTransform> allCardLists = new List<RectTransform>();
        if (originalCardPositions != null)
        {
            allCardLists.AddRange(originalCardPositions);
        }
        if (cardPositionsAttack != null)
        {
            allCardLists.AddRange(cardPositionsAttack);
        }
        if (cardPositionsBALANCE != null)
        {
            allCardLists.AddRange(cardPositionsBALANCE);
        }
        if (cardPositionsDEFENSE != null)
        {
            allCardLists.AddRange(cardPositionsDEFENSE);
        }
        if (cardPositions != null)
        {
            allCardLists.AddRange(cardPositions);
        }
        
        foreach (var rect in allCardLists)
        {
            if (rect == null) continue;
            GameObject cardObj = rect.gameObject;
            if (cardObj != null && !cardsInHandSet.Contains(cardObj))
            {
                cardObj.SetActive(false);
            }
        }
    }
}
