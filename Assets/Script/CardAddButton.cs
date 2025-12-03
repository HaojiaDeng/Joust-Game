using UnityEngine;
using UnityEngine.UI;

public class CardAddButton : MonoBehaviour
{
    private void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => CardManagement.Instance.AddCard(gameObject));
        
    }
}
