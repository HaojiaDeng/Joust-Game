using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite artwork;
}
