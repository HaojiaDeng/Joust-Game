using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardAnimation : MonoBehaviour
{
    public static CardAnimation Instance;

    //The initial position of the cards
    [SerializeField] private Vector2 startPos = new Vector2(0, 150);
    //Time to move each card
    [SerializeField] private float moveDuration = 1f;

    private List<Vector2> defaultTargetPositions = new List<Vector2>()
    {
        new Vector2(168, -360),
        new Vector2(488, -360),
        new Vector2(808, -360),
        new Vector2(1128, -360),
        new Vector2(1448, -360),
        new Vector2(1768, -360),
        new Vector2(168, -760),
        new Vector2(488, -760),
        new Vector2(808, -760),
        new Vector2(1128, -760),
        new Vector2(1448, -760),
        new Vector2(1768, -760)
    };

    //deck position
    public List<RectTransform> deckRectran;

    //Initial position of the deck
    private List<Vector2> deckInitPos = new List<Vector2>
    {
        new Vector2(-400f,0),
        new Vector2(0f,0),
        new Vector2(400f,0)
    };

    //Initial height and width of the deck
    private Vector2 deckInitWH = new Vector2(270f, 460f); 
    private List<Vector2> deckPos = new List<Vector2>
    {
        new Vector2(880f,300f),
        new Vector2(880f,100f),
        new Vector2(880f,-100f)
    };

    //Height and width of the deck
    private Vector2 deckWH = new Vector2(120f, 170f); 

    private Vector2 playPos = new Vector2(0f, 150f);

    private void Awake()
    {
        Instance = this;
    }

    public void StartPickingCards(bool isStart)
    {

    }


    public void StartCardMoveAnimation(List<RectTransform> cardList)
    {
        if (cardList == null || cardList.Count == 0)
        {
            Debug.LogError("The passed-in card list cannot be empty");
            return;
        }

        if (defaultTargetPositions.Count < cardList.Count)
        {
            Debug.LogWarning($"Not enough target positions (only {defaultTargetPositions.Count}) to match {cardList.Count} cards.");
            return;
        }

        // Initialize all cards to their initial positions
        foreach (var card in cardList)
        {
            if (card != null)
                card.anchoredPosition = startPos;
        }


        StartCoroutine(MoveCardsInSequence(cardList));
    }


    private IEnumerator MoveCardsInSequence(List<RectTransform> cardList)
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            var card = cardList[i];
            if (card == null)
            {
                Debug.LogWarning($"Card {i + 1} is null, skipping movement.");
                continue;
            }

            var targetPos = defaultTargetPositions[i];
            yield return StartCoroutine(MoveSingleCard(card, targetPos));
        }

        Debug.Log("All cards have finished moving.");
    }

    // Linear movement logic for a single card
    private IEnumerator MoveSingleCard(RectTransform card, Vector2 targetPos)
    {
        float elapsedTime = 0f;
        Vector2 startPosition = card.anchoredPosition;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            card.anchoredPosition = Vector2.Lerp(startPosition, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        card.anchoredPosition = targetPos;
    }

    // Play card animation
    public void PlayCard(RectTransform tran)
    {
        StartCoroutine(MoveSingleCard(tran, playPos));
    }
}