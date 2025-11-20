using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponCardUI : MonoBehaviour
{
    public Image artworkImage;
    public TMP_Text nameText;

    private WeaponData weapon;

    public void Setup(WeaponData data)
    {
        weapon = data;
        artworkImage.sprite = data.artwork;
        nameText.text = data.weaponName;
    }

    public void OnClick()
    {
        Debug.Log("Selected weapon: " + weapon.weaponName);
    }
}
