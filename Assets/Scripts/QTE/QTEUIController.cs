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
        promptPanel.SetActive(true);
        promptText.text = key.ToString();
        if (icon != null && promptImage != null) 
            promptImage.sprite = icon;
    }
    
    public void HidePrompt()
    {
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