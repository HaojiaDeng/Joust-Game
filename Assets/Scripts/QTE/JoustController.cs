using UnityEngine;
using System.Collections;

public class JoustController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform enemy;
    [SerializeField] private Camera mainCamera;

    private CardData currentCard;

    public void StartJoust(CardData card)
    {
        currentCard = card;
        StartCoroutine(JoustSequence());
    }

    private IEnumerator JoustSequence()
    {
        // Play charge animations
        yield return StartCoroutine(ChargeSequence());

        // Start QTE
        QTEManager qteManager = GetComponent<QTEManager>();
        yield return StartCoroutine(qteManager.ExecuteQTE(currentCard.qtePattern));
        QTEResult result = qteManager.GetLastQTEResult();

        // Handle result
        HandleJoustOutcome(result);
    }

    private IEnumerator ChargeSequence()
    {
        // Animate player and enemy charging
        // Move camera
        yield return new WaitForSeconds(2f); // Duration of charge
    }

    private void HandleJoustOutcome(QTEResult result)
    {
        int finalDamage = Mathf.RoundToInt(currentCard.baseDamage * result.damageMultiplier);

        if (result.isPerfect)
        {
            // Play success animation
        }
        else
        {
            // Play failure animation
        }

        // Apply damage, update game state
    }
}