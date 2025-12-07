using UnityEngine;
using System.Collections;

public class QTEPhaseManager
{
    private CardHandManager cardHandManager;
    private GameObject qteUI;
    private bool qteUIWasActive;
    private BossCardDisplay bossCardDisplay;
    private GameObject qteReadyText;
    
    public void Initialize(CardHandManager handManager, GameObject qteUIObject, bool qteUIWasActiveState, BossCardDisplay bossCardDisplayRef, GameObject qteReadyTextObj)
    {
        cardHandManager = handManager;
        qteUI = qteUIObject;
        qteUIWasActive = qteUIWasActiveState;
        bossCardDisplay = bossCardDisplayRef;
        qteReadyText = qteReadyTextObj;
    }
    
    public void SetQTEUIState(GameObject qteUIObject, bool wasActive)
    {
        qteUI = qteUIObject;
        qteUIWasActive = wasActive;
    }
    
    public void OnQTESuccess(CardData playerCard, QTEResult qteResult)
    {
        RestoreUIAndCamera();

        if (playerCard != null && BossStats.Instance != null && BossStats.Instance.IsActive())
        {
            int finalDamage = Mathf.RoundToInt(playerCard.baseDamage * qteResult.damageMultiplier);
            
            CardStats.CardData bossCard = BossBattle.Instance != null ? BossBattle.Instance.GetBossCard() : null;
            
            string advantageText = "";
            if (bossCard != null && WeaponAdvantageSystem.Instance != null)
            {
                WeaponCategory playerWeapon = playerCard.category;
                WeaponCategory bossWeapon = DamageSystem.ConvertCardTypeToWeaponCategory(bossCard.cardType);
                
                float advantageMultiplier = WeaponAdvantageSystem.Instance.GetAdvantageMultiplier(playerWeapon, bossWeapon);
                advantageText = WeaponAdvantageSystem.Instance.GetAdvantageText(playerWeapon, bossWeapon);
            }
            
            if (DamageSystem.Instance != null)
            {
                DamageSystem.Instance.ApplyBossDamageWithVisualFeedback(finalDamage, advantageText);
            }
        }

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.IncrementRound();
        }

        if (cardHandManager != null)
        {
            cardHandManager.ShowHiddenHand();
        }
    }

    public void OnQTEFailed(CardData playerCard)
    {
        RestoreUIAndCamera();

        if (playerCard != null && GameLoopManager.Instance != null && BossBattle.Instance != null)
        {
            CardStats.CardData bossCard = BossBattle.Instance.GetBossCard();
            
            if (bossCard != null)
            {
                WeaponCategory bossWeapon = DamageSystem.ConvertCardTypeToWeaponCategory(bossCard.cardType);
                WeaponCategory playerWeapon = playerCard.category;
                
                float advantageMultiplier = 1.0f;
                if (WeaponAdvantageSystem.Instance != null)
                {
                    advantageMultiplier = WeaponAdvantageSystem.Instance.GetAdvantageMultiplier(bossWeapon, playerWeapon);
                }
                
                int baseDamage = 2;
                int finalDamage = Mathf.RoundToInt(baseDamage * advantageMultiplier);
                if (finalDamage < 1) finalDamage = 1;
                
                if (DamageSystem.Instance != null)
                {
                    DamageSystem.Instance.ApplyPlayerDamageWithVisualFeedback(finalDamage);
                }
                
                string advantageText = "";
                if (WeaponAdvantageSystem.Instance != null)
                {
                    advantageText = WeaponAdvantageSystem.Instance.GetAdvantageText(bossWeapon, playerWeapon);
                }
                
                string damageMessage = $"Player takes {finalDamage} damage!";
                if (!string.IsNullOrEmpty(advantageText))
                {
                    damageMessage += $" ({advantageText})";
                }
                
                GameLoopManager.Instance.ShowCenterMessage(damageMessage, 2f);
            }
            else
            {
                GameLoopManager.Instance.TakeDamage();
            }
        }

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.IncrementRound();
        }

        if (cardHandManager != null)
        {
            cardHandManager.ShowHiddenHand();
        }
    }

    public void EnterQTE()
    {
        bossCardDisplay?.CleanupBossCard();

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.HideUIForQTE();
        }
    }
    
    public System.Collections.IEnumerator StartQTESetup(GameObject qteUIObject, CardData currentPlayerCardData)
    {
        if (qteUIObject != null)
        {
            qteUI = qteUIObject;
        }
        
        if (qteUI == null)
        {
            qteUI = GameObject.Find("QTE");
            if (qteUI == null)
            {
                yield break;
            }
        }

        RandomEncounterManager randomEncounter = qteUI.GetComponentInChildren<RandomEncounterManager>();
        if (randomEncounter != null)
        {
            randomEncounter.enabled = false;
        }

        EnterQTE();
        
        yield return new WaitForSeconds(0.1f);
        
        if (qteUI == null)
        {
            yield break;
        }
        
        qteUIWasActive = qteUI.activeSelf;
        SetQTEUIState(qteUI, qteUIWasActive);
        qteUI.SetActive(true);
        
        Transform playerTransform = qteUI.transform.Find("Player");
        Transform enemyTransform = qteUI.transform.Find("Enemy");
        
        if (playerTransform != null)
        {
            playerTransform.gameObject.SetActive(true);
            
            SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.sortingOrder = 10;
            }
        }
        
        if (enemyTransform != null)
        {
            enemyTransform.gameObject.SetActive(true);
            
            SpriteRenderer enemySprite = enemyTransform.GetComponent<SpriteRenderer>();
            if (enemySprite != null)
            {
                enemySprite.sortingOrder = 10;
            }
        }
        
        Canvas canvas = qteUI.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GameObject bgObj = GameObject.Find("Bg");
            if (bgObj != null)
            {
                Canvas bgCanvas = bgObj.GetComponentInParent<Canvas>();
                if (bgCanvas != null && bgCanvas != canvas)
                {
                    if (canvas.sortingOrder <= bgCanvas.sortingOrder)
                    {
                        canvas.sortingOrder = bgCanvas.sortingOrder + 10;
                    }
                }
            }
        }
        
        yield return new WaitForEndOfFrame();
        
        if (qteUI == null)
        {
            yield break;
        }
        
        QTEUIController uiController = qteUI.GetComponentInChildren<QTEUIController>(true);
        if (uiController != null)
        {
            uiController.HidePrompt();
        }
        
        GameObject promptPanel = GameObject.Find("PromptPanel");
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
        
        GameObject promptTextObj = GameObject.Find("PromptText");
        if (promptTextObj != null)
        {
            TMPro.TextMeshProUGUI promptText = promptTextObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (promptText != null)
            {
                promptText.text = "";
            }
        }
        
        if (qteUI != null)
        {
            UnityEngine.UI.RawImage[] rawImages = qteUI.GetComponentsInChildren<UnityEngine.UI.RawImage>(true);
            foreach (var rawImage in rawImages)
            {
                if (rawImage == null || rawImage.gameObject == null) continue;
                
                if (rawImage.texture != null && rawImage.texture is RenderTexture)
                {
                    rawImage.gameObject.SetActive(false);
                }
            }
            
            Transform[] allChildren = qteUI.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child == null || child.gameObject == null) continue;
                
                if (child.name.Contains("Display", System.StringComparison.OrdinalIgnoreCase) || 
                    child.name.Contains("Camera", System.StringComparison.OrdinalIgnoreCase))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        if (qteReadyText != null)
        {
            UnityEngine.UI.RawImage readyTextRawImage = qteReadyText.GetComponent<UnityEngine.UI.RawImage>();
            if (readyTextRawImage != null)
            {
                if (readyTextRawImage.texture is RenderTexture)
                {
                    qteReadyText.SetActive(false);
                }
            }
            if (qteReadyText.name.Contains("Display", System.StringComparison.OrdinalIgnoreCase))
            {
                qteReadyText.SetActive(false);
            }
        }
            
        if (BossBattle.Instance != null && BossBattle.Instance.IsBossBattleActive())
        {
            JoustController joustController = qteUI.GetComponentInChildren<JoustController>(true);
            if (joustController == null)
            {
                joustController = Object.FindFirstObjectByType<JoustController>();
            }
            
            if (joustController != null && currentPlayerCardData != null)
            {
                joustController.StartJoust(currentPlayerCardData);
            }
        }
    }

    private void RestoreUIAndCamera()
    {
        if (qteUI != null)
        {
            qteUI.SetActive(qteUIWasActive);
        }
        
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.RestoreUIAfterQTE();
        }
    }
}

