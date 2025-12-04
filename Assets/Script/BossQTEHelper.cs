using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossQTEHelper : MonoBehaviour
{
    public static BossQTEHelper Instance;

    private static readonly KeyCode[] allKeys = new KeyCode[]
    {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
        KeyCode.Space
    };

    private void Awake()
    {
        Instance = this;
    }

    public CardData CreateBossCardData(CardStats.CardData bossCard)
    {
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.cardName = bossCard.cardType.ToString();
        cardData.baseDamage = 2;
        cardData.defenseValue = 0;
        
        QTEType[] qteTypes = new QTEType[]
        {
            QTEType.Sequence,
            QTEType.Directional,
            QTEType.Rhythm,
            QTEType.ButtonMash,
            QTEType.HoldAndRelease
        };

        QTEType selectedType = qteTypes[Random.Range(0, qteTypes.Length)];
        cardData.qtePattern = GenerateQTEPattern(selectedType);

        return cardData;
    }

    private QTEPattern GenerateQTEPattern(QTEType type)
    {
        QTEPattern pattern = ScriptableObject.CreateInstance<QTEPattern>();
        pattern.qteType = type;
        pattern.gapBetweenInputs = 0.5f;
        pattern.mustBeConsecutive = false;
        pattern.perfectBonus = 1.5f;
        pattern.failurePenalty = 0.5f;

        int keyCount = 4;

        switch (type)
        {
            case QTEType.Sequence:
            case QTEType.Directional:
            case QTEType.Rhythm:
                pattern.inputSequence = GenerateRandomInputSequence(keyCount, type);
                pattern.minimumSuccessfulInputs = Mathf.Max(1, keyCount - 1);
                break;

            case QTEType.ButtonMash:
                float mashDuration = 2.5f;
                pattern.inputSequence = new List<QTEInput>
                {
                    new QTEInput
                    {
                        requiredKey = GetRandomKey(),
                        windowDuration = mashDuration,
                        delayBeforeThisPrompt = 0.5f
                    }
                };
                pattern.minimumSuccessfulInputs = 6;
                break;

            case QTEType.HoldAndRelease:
                float holdWindow = 2.5f;
                pattern.inputSequence = new List<QTEInput>
                {
                    new QTEInput
                    {
                        requiredKey = GetRandomKey(),
                        windowDuration = holdWindow,
                        delayBeforeThisPrompt = 0.5f
                    }
                };
                pattern.minimumSuccessfulInputs = 1;
                pattern.gapBetweenInputs = 1.5f;
                break;
        }

        return pattern;
    }

    private List<QTEInput> GenerateRandomInputSequence(int count, QTEType type)
    {
        List<QTEInput> inputs = new List<QTEInput>();
        float baseWindow = 1.0f;

        for (int i = 0; i < count; i++)
        {
            QTEInput input = new QTEInput
            {
                requiredKey = GetRandomKey(),
                windowDuration = baseWindow,
                delayBeforeThisPrompt = i == 0 ? 0.5f : 0f
            };
            inputs.Add(input);
        }

        return inputs;
    }

    private KeyCode GetRandomKey()
    {
        return allKeys[Random.Range(0, allKeys.Length)];
    }
}

