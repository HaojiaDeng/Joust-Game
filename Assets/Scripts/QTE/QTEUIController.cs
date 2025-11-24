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
        Debug.Log($"ShowPrompt called for key: {key}");
        
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
        
        Debug.Log($"Setting panel active and text to: {key.ToString()}");
        promptPanel.SetActive(true);
        promptText.text = key.ToString();
        promptText.color = Color.white; // Reset color
        
        if (icon != null && promptImage != null) 
            promptImage.sprite = icon;
            
        Debug.Log("ShowPrompt complete!");
    }
    
    public void HidePrompt()
    {
        Debug.Log("HidePrompt called");
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }
    
    public void ShowSuccess()
    {
        Debug.Log("ShowSuccess called");
        // Visual feedback for successful input
        if (promptText != null)
            promptText.color = Color.green;
    }
    
    public void ShowFailure()
    {
        Debug.Log("ShowFailure called");
        // Visual feedback for failed input
        if (promptText != null)
            promptText.color = Color.red;
    }
}