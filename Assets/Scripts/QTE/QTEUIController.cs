using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QTEUIController : MonoBehaviour
{
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private Image promptImage;
    [SerializeField] private TextMeshProUGUI promptText;
    
    public void ShowPrompt(KeyCode key, Sprite icon = null)
    {
        
        if (promptPanel == null)
        {
            Debug.LogError("PromptPanel is null!");
            return;
        }
        
        if (promptText == null)
        {
            Debug.LogError("PromptText is null!");
            return;
        }
        
        promptPanel.SetActive(true);
        promptText.text = key.ToString();
        promptText.color = Color.white; // Reset color
        
        if (icon != null && promptImage != null) 
            promptImage.sprite = icon;
            
    }
    
    public void HidePrompt()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }
    
    public void ShowSuccess()
    {
        // Visual feedback for successful input
        if (promptText != null)
            promptText.color = Color.green;
    }
    
    public void ShowFailure()
    {
        // Visual feedback for failed input
        if (promptText != null)
            promptText.color = Color.red;
    }
}