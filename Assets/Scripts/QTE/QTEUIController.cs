using UnityEngine;
using UnityEngine.UI;

public class QTEUIController : MonoBehaviour
{
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private Image promptImage;
    [SerializeField] private Text promptText;

    public void ShowPrompt(KeyCode key, Sprite icon = null)
    {
        promptPanel.SetActive(true);
        promptText.text = key.ToString();
        if (icon != null) promptImage.sprite = icon;
    }

    public void HidePrompt()
    {
        promptPanel.SetActive(false);
    }

    public void ShowSuccess()
    {
        // Visual feedback for successful input
    }

    public void ShowFailure()
    {
        // Visual feedback for failed input
    }
}
