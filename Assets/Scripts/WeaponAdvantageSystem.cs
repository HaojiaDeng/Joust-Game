using UnityEngine;

public class WeaponAdvantageSystem : MonoBehaviour
{
    public static WeaponAdvantageSystem Instance;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // Returns damage multiplier based on advantage
    public float GetAdvantageMultiplier(WeaponCategory playerWeapon, WeaponCategory enemyWeapon)
    {
        // SwordAxe beats BowLance
        if (playerWeapon == WeaponCategory.SwordAxe && enemyWeapon == WeaponCategory.BowLance)
            return 1.5f;
        
        // BowLance beats Shield
        if (playerWeapon == WeaponCategory.BowLance && enemyWeapon == WeaponCategory.Shield)
            return 1.5f;
        
        // Shield beats SwordAxe
        if (playerWeapon == WeaponCategory.Shield && enemyWeapon == WeaponCategory.SwordAxe)
            return 1.5f;
        
        // Disadvantage cases
        if (playerWeapon == WeaponCategory.BowLance && enemyWeapon == WeaponCategory.SwordAxe)
            return 0.5f;
        
        if (playerWeapon == WeaponCategory.Shield && enemyWeapon == WeaponCategory.BowLance)
            return 0.5f;
        
        if (playerWeapon == WeaponCategory.SwordAxe && enemyWeapon == WeaponCategory.Shield)
            return 0.5f;
        
        // Neutral matchup
        return 1.0f;
    }
    
    public string GetAdvantageText(WeaponCategory playerWeapon, WeaponCategory enemyWeapon)
    {
        float multiplier = GetAdvantageMultiplier(playerWeapon, enemyWeapon);
        
        if (multiplier > 1.0f)
            return "ADVANTAGE!";
        else if (multiplier < 1.0f)
            return "DISADVANTAGE!";
        else
            return "NEUTRAL";
    }
}