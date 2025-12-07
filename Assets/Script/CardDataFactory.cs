using System.Collections.Generic;
using UnityEngine;

public static class CardDataFactory
{
    public static CardData CreateCardDataFromGameObject(GameObject cardObj)
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

        QTEPattern pattern = Object.FindFirstObjectByType<RandomEncounterManager>() != null
            ? GenerateRandomQTEPattern()
            : CreateDefaultQTEPattern();
        cardData.qtePattern = pattern;

        return cardData;
    }

    public static CardStats.CardType GetCardTypeFromName(string name)
    {
        string lowerName = name.ToLower();
        if (lowerName.Contains("axe")) return CardStats.CardType.Axe;
        if (lowerName.Contains("bow")) return CardStats.CardType.Bow;
        if (lowerName.Contains("lance")) return CardStats.CardType.Lance;
        if (lowerName.Contains("shield")) return CardStats.CardType.Shield;
        if (lowerName.Contains("sword")) return CardStats.CardType.Sword;
        return CardStats.CardType.Sword;
    }

    public static QTEPattern GenerateRandomQTEPattern()
    {
        if (Object.FindFirstObjectByType<RandomEncounterManager>() == null || GameLoopManager.Instance == null)
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

    public static QTEPattern CreateDefaultQTEPattern()
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
}

