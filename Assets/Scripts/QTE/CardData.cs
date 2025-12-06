using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/WeaponCard")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public string description;
    public Sprite cardArt;
    public int baseDamage = 1;

    [Header("Weapon Properties")]
    public WeaponCategory category;
    
    [Header("Combat Stats")]
    public int minDamage = 1;
    public int maxDamage = 3;
    public int defenseValue;
    
    [Header("QTE Configuration")]
    public QTEPattern qtePattern;
    
    [Header("Animations")]
    public AnimationClip chargeAnimation;
    public AnimationClip successAnimation;
    public AnimationClip failureAnimation;
}