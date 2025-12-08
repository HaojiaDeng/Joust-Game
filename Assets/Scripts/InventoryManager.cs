using System.Collections.Generic;
using UnityEngine;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<WeaponData> playerInventory = new List<WeaponData>();

    private void Awake()
    {
        Instance = this;
    }
}
