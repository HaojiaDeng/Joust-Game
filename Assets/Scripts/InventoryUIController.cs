using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    public GameObject weaponCardPrefab;
    public Transform gridParent;
    public float animationDuration = 0.5f;
    public float bottomOffset = 150f;
    public float horizontalOffset = 0f;
    public float fanSpreadAngle = 30f;
    public float fanRadius = 150f;
    public float fanVerticalOffset = 30f;
    private List<GameObject> weaponCards = new List<GameObject>();
    private List<Vector2> expandedPositions = new List<Vector2>();
    private List<float> collapsedRotations = new List<float>();
    private Vector2 collapsedCenterPosition;
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
        StartCoroutine(StoreExpandedPositions());
    }

    private IEnumerator StoreExpandedPositions()
    {
        yield return new WaitForEndOfFrame();
        RectTransform parentRect = gridParent.GetComponent<RectTransform>();
        float panelHeight = parentRect.rect.height;
        foreach (var card in weaponCards)
        {
            RectTransform rt = card.GetComponent<RectTransform>();
            Vector3 worldPos = rt.position;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.position = worldPos;
            expandedPositions.Add(rt.anchoredPosition);
        }
        UnityEngine.UI.GridLayoutGroup gridLayout = gridParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.enabled = false;
        }
        float yPos = -panelHeight / 2 + bottomOffset;
        collapsedCenterPosition = new Vector2(horizontalOffset, yPos);
        CalculateFeatherPositions();
        CollapseImmediate();
    }

    private void CalculateFeatherPositions()
    {
        int cardCount = weaponCards.Count;
        for (int i = 0; i < cardCount; i++)
        {
            float normalizedPosition = (cardCount == 1) ? 0.5f : (float)i / (cardCount - 1);
            float angle = Mathf.Lerp(-fanSpreadAngle / 2f, fanSpreadAngle / 2f, normalizedPosition);
            collapsedRotations.Add(angle);
        }
    }

    private void CollapseImmediate()
    {
        for (int i = 0; i < weaponCards.Count; i++)
        {
            RectTransform rt = weaponCards[i].GetComponent<RectTransform>();
            float angle = collapsedRotations[i];
            float angleRad = angle * Mathf.Deg2Rad;
            float xOffset = Mathf.Sin(angleRad) * fanRadius;
            float yOffset = Mathf.Cos(angleRad) * fanVerticalOffset;
            rt.anchoredPosition = collapsedCenterPosition + new Vector2(xOffset, yOffset);
            rt.localRotation = Quaternion.Euler(0, 0, angle);
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
        List<Vector2> startPositions = new List<Vector2>();
        List<Quaternion> startRotations = new List<Quaternion>();
        foreach (var card in weaponCards)
        {
            RectTransform rt = card.GetComponent<RectTransform>();
            startPositions.Add(rt.anchoredPosition);
            startRotations.Add(rt.localRotation);
        }
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            for (int i = 0; i < weaponCards.Count; i++)
            {
                RectTransform rt = weaponCards[i].GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.Lerp(startPositions[i], expandedPositions[i], easedT);
                rt.localRotation = Quaternion.Lerp(startRotations[i], Quaternion.identity, easedT);
            }
            yield return null;
        }
        for (int i = 0; i < weaponCards.Count; i++)
        {
            RectTransform rt = weaponCards[i].GetComponent<RectTransform>();
            rt.anchoredPosition = expandedPositions[i];
            rt.localRotation = Quaternion.identity;
        }
        isExpanded = true;
        isAnimating = false;
    }

    private IEnumerator AnimateCollapse()
    {
        isAnimating = true;
        float elapsedTime = 0f;
        List<Vector2> startPositions = new List<Vector2>();
        foreach (var card in weaponCards)
        {
            startPositions.Add(card.GetComponent<RectTransform>().anchoredPosition);
        }
        List<Vector2> targetPositions = new List<Vector2>();
        for (int i = 0; i < weaponCards.Count; i++)
        {
            float angle = collapsedRotations[i];
            float angleRad = angle * Mathf.Deg2Rad;
            float xOffset = Mathf.Sin(angleRad) * fanRadius;
            float yOffset = Mathf.Cos(angleRad) * fanVerticalOffset;
            targetPositions.Add(collapsedCenterPosition + new Vector2(xOffset, yOffset));
        }
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float easedT = t * t * t;
            for (int i = 0; i < weaponCards.Count; i++)
            {
                RectTransform rt = weaponCards[i].GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], easedT);
                float targetAngle = collapsedRotations[i];
                rt.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 0, targetAngle), easedT);
            }
            yield return null;
        }
        for (int i = 0; i < weaponCards.Count; i++)
        {
            RectTransform rt = weaponCards[i].GetComponent<RectTransform>();
            rt.anchoredPosition = targetPositions[i];
            rt.localRotation = Quaternion.Euler(0, 0, collapsedRotations[i]);
        }
        isExpanded = false;
        isAnimating = false;
    }
}
