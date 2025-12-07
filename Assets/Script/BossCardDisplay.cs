using UnityEngine;
using UnityEngine.UI;

public class BossCardDisplay
{
    private GameObject bossCardObject;
    
    private RectTransform attackParent;
    private Vector3 bossCardOffset;
    private GameObject axePrefab;
    private GameObject bowPrefab;
    private GameObject lancePrefab;
    private GameObject shieldPrefab;
    private GameObject swordPrefab;
    
    public void Initialize(
        RectTransform parent,
        Vector3 offset,
        GameObject axe,
        GameObject bow,
        GameObject lance,
        GameObject shield,
        GameObject sword)
    {
        attackParent = parent;
        bossCardOffset = offset;
        axePrefab = axe;
        bowPrefab = bow;
        lancePrefab = lance;
        shieldPrefab = shield;
        swordPrefab = sword;
    }
    
    public void ShowBossCard()
    {
        if (BossManager.Instance == null)
        {
            return;
        }

        CardStats.CardData bossCard = BossManager.Instance.SelectBossCard();
        if (bossCard == null)
        {
            return;
        }

        CleanupBossCard();

        GameObject cardPrefab = GetCardPrefabByType(bossCard.cardType);
        if (cardPrefab == null)
        {
            return;
        }

        bossCardObject = Object.Instantiate(cardPrefab, attackParent);
        RectTransform bossCardRect = bossCardObject.GetComponent<RectTransform>();
        if (bossCardRect != null)
        {
            bossCardRect.localPosition = bossCardOffset;
        }
        else
        {
            bossCardObject.transform.localPosition = bossCardOffset;
        }

        Button bossButton = bossCardObject.GetComponent<Button>();
        if (bossButton != null)
        {
            bossButton.enabled = false;
        }
    }
    
    public void CleanupBossCard()
    {
        if (bossCardObject != null)
        {
            Object.Destroy(bossCardObject);
            bossCardObject = null;
        }
    }
    
    private GameObject GetCardPrefabByType(CardStats.CardType cardType)
    {
        switch (cardType)
        {
            case CardStats.CardType.Axe:
                return axePrefab;
            case CardStats.CardType.Lance:
                return lancePrefab;
            case CardStats.CardType.Bow:
                return bowPrefab;
            case CardStats.CardType.Sword:
                return swordPrefab;
            case CardStats.CardType.Shield:
                return shieldPrefab;
            default:
                return null;
        }
    }
}
