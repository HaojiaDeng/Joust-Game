using UnityEngine;
using System.Collections.Generic;

public class CardStats : MonoBehaviour
{
    public static CardStats Instance;

    public enum CardType
    {
        Axe,
        Lance,
        Bow,
        Sword,
        Shield
    }

    [System.Serializable]
    public class CardData
    {
        public CardType cardType;

        public CardData(CardType type)
        {
            cardType = type;
        }
    }

    private static Dictionary<CardType, CardData> cardStatsDict;

    private void Awake()
    {
        Instance = this;
        InitializeCardStats();
    }

    private void InitializeCardStats()
    {
        if (cardStatsDict == null)
        {
            cardStatsDict = new Dictionary<CardType, CardData>
            {
                { CardType.Axe, new CardData(CardType.Axe) },
                { CardType.Lance, new CardData(CardType.Lance) },
                { CardType.Bow, new CardData(CardType.Bow) },
                { CardType.Sword, new CardData(CardType.Sword) },
                { CardType.Shield, new CardData(CardType.Shield) }
            };
        }
    }

    public static CardData GetCardStats(CardType cardType)
    {
        if (cardStatsDict == null)
        {
            InitializeStaticStats();
        }

        if (cardStatsDict.TryGetValue(cardType, out CardData stats))
        {
            return stats;
        }

        return null;
    }

    public static CardData GetCardStatsByName(string cardName)
    {
        CardType type = GetCardTypeFromName(cardName);
        return GetCardStats(type);
    }

    private static CardType GetCardTypeFromName(string cardName)
    {
        switch (cardName.ToLower())
        {
            case "axe":
                return CardType.Axe;
            case "lance":
                return CardType.Lance;
            case "bow":
                return CardType.Bow;
            case "sword":
                return CardType.Sword;
            case "shield":
                return CardType.Shield;
            default:
                return CardType.Sword;
        }
    }

    private static void InitializeStaticStats()
    {
        cardStatsDict = new Dictionary<CardType, CardData>
        {
            { CardType.Axe, new CardData(CardType.Axe) },
            { CardType.Lance, new CardData(CardType.Lance) },
            { CardType.Bow, new CardData(CardType.Bow) },
            { CardType.Sword, new CardData(CardType.Sword) },
            { CardType.Shield, new CardData(CardType.Shield) }
        };
    }

}

