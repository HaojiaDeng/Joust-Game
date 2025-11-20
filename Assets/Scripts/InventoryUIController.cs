using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    public GameObject weaponCardPrefab;
    public Transform gridParent;
    public float animationDuration = 0.5f;
    public Vector2 stackedPosition = Vector2.zero; // Position where cards stack together
    
    private List<GameObject> weaponCards = new List<GameObject>();
    private List<Vector2> expandedPositions = new List<Vector2>();
    private bool isExpanded = false;
    private bool isAnimating = false;

    private void Start()
    {
        CreateCards();
    }

    private void CreateCards()
    {
        foreach (var weapon in InventoryManager.Instance.playerInventory)
        {
            GameObject card = Instantiate(weaponCardPrefab, gridParent);
            card.GetComponent<WeaponCardUI>().Setup(weapon);
            weaponCards.Add(card);
        }

        // Wait one frame for layout to position cards, then store positions
        StartCoroutine(StoreExpandedPositions());
    }

    private IEnumerator StoreExpandedPositions()
    {
        yield return new WaitForEndOfFrame();
        
        // Store the grid layout positions
        foreach (var card in weaponCards)
        {
            RectTransform rt = card.GetComponent<RectTransform>();
            expandedPositions.Add(rt.anchoredPosition);
        }

        // Start in collapsed state
        CollapseImmediate();
    }

    private void CollapseImmediate()
    {
        foreach (var card in weaponCards)
        {
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = stackedPosition;
        }
        isExpanded = false;
    }

    public void ToggleInventory()
    {
        if (isAnimating) return;

        if (isExpanded)
        {
            StartCoroutine(AnimateCollapse());
        }
        else
        {
            StartCoroutine(AnimateExpand());
        }
    }

    private IEnumerator AnimateExpand()
    {
        isAnimating = true;
        float elapsedTime = 0f;

        // Store start positions
        List<Vector2> startPositions = new List<Vector2>();
        foreach (var card in weaponCards)
        {
            startPositions.Add(card.GetComponent<RectTransform>().anchoredPosition);
        }

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Ease out

            for (int i = 0; i < weaponCards.Count; i++)
            {
                RectTransform rt = weaponCards[i].GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.Lerp(startPositions[i], expandedPositions[i], easedT);
            }

            yield return null;
        }

        // Ensure final positions
        for (int i = 0; i < weaponCards.Count; i++)
        {
            weaponCards[i].GetComponent<RectTransform>().anchoredPosition = expandedPositions[i];
        }

        isExpanded = true;
        isAnimating = false;
    }

    private IEnumerator AnimateCollapse()
    {
        isAnimating = true;
        float elapsedTime = 0f;

        // Store start positions
        List<Vector2> startPositions = new List<Vector2>();
        foreach (var card in weaponCards)
        {
            startPositions.Add(card.GetComponent<RectTransform>().anchoredPosition);
        }

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float easedT = t * t * t; // Ease in

            for (int i = 0; i < weaponCards.Count; i++)
            {
                RectTransform rt = weaponCards[i].GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.Lerp(startPositions[i], stackedPosition, easedT);
            }

            yield return null;
        }

        // Ensure final positions
        foreach (var card in weaponCards)
        {
            card.GetComponent<RectTransform>().anchoredPosition = stackedPosition;
        }

        isExpanded = false;
        isAnimating = false;
    }
}
