using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public CardStats.CardType GetRandomBossCard()
    {
        CardStats.CardType[] cards = {
            CardStats.CardType.Axe,
            CardStats.CardType.Lance,
            CardStats.CardType.Bow,
            CardStats.CardType.Sword,
            CardStats.CardType.Shield
        };

        return cards[Random.Range(0, cards.Length)];
    }

    public CardStats.CardData SelectBossCard()
    {
        CardStats.CardType selectedType = GetRandomBossCard();
        CardStats.CardData cardData = CardStats.GetCardStats(selectedType);

        if (BossStats.Instance != null)
        {
            BossStats.Instance.SetCurrentCard(cardData);
        }

        Debug.Log($"Boss selected card: {selectedType}");

        return cardData;
    }

    public void StartBossBattle()
    {
        if (BossStats.Instance != null && !BossStats.Instance.IsActive())
        {
            BossStats.Instance.InitializeBoss();
        }
    }
}

