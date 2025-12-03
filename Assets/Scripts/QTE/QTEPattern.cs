using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QTEPattern", menuName = "QTE/QTEPattern")]
public class QTEPattern : ScriptableObject
{
    public string patternName;
    public QTEType qteType;

    public List<QTEInput> inputSequence;
    public float globalInputDelay;
    public float gapBetweenInputs;

    public int minimumSuccessfulInputs;
    public bool mustBeConsecutive;

    public float perfectBonus = 1.5f;
    public float failurePenalty = 0.5f;
}