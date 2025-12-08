using UnityEngine;

public enum WeaponCategory
{
    SwordAxe,
    BowLance,
    Shield
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite artwork;
    
    [Header("Combat Properties")]
    public WeaponCategory category;
    public int minDamage = 1;
    public int maxDamage = 3;
}