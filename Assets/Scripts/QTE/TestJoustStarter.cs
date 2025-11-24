using UnityEngine;

public class TestJoustStarter : MonoBehaviour
{
    [SerializeField] private CardData testCard;
    
    public void StartTestJoust()
    {
        if (testCard != null)
        {
            GetComponent<JoustController>().StartJoust(testCard);
        }
        else
        {
            Debug.LogError("No test card assigned!");
        }
    }
}