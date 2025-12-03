using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/WeaponCard")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public string description;
    public Sprite cardArt;

    [Header("Combat Stats")]
    public int baseDamage;
    public int defenseValue;

    [Header("QTE Configuration")]
    public QTEPattern qtePattern;

    [Header("Animations")]
    public AnimationClip chargeAnimation;
    public AnimationClip successAnimation;
    public AnimationClip failureAnimation;
}