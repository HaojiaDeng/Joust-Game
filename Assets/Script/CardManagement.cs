using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardManagement : MonoBehaviour
{
    public static CardManagement Instance;
    public GameObject AXE;
    public GameObject Bow;
    public GameObject Lance;
    public GameObject Shield;
    public GameObject Sword;
    public Transform HandsPlayed;
    public GameObject UI4;
    public GameObject UI20;
    public GameObject UI21;
    public GameObject UI22;
    public GameObject clickEffect;
    public RectTransform attackParent;
    public Text logText;
    public List<RectTransform> cardPositionsAttack = new List<RectTransform>();
    public List<RectTransform> cardPositionsBALANCE = new List<RectTransform>();
    public List<RectTransform> cardPositionsDEFENSE = new List<RectTransform>();
    public List<RectTransform> cardPositions = new List<RectTransform>();
    private int vlaue = 0;
    private int logInt = 12;
    public List<GameObject> cards = new List<GameObject>();
    private GameObject PlayACardObject;

    public int NumberBattleCards = 0;

    private void Awake()
    {
        Instance = this;
        logText.text = "Card Left:" + logInt.ToString();
    }

    public void StartGame(string cardName)
    {
        switch (cardName)
        {
            case "Attack":
                cardPositions = cardPositionsAttack;
                CardAnimation.Instance.StartCardMoveAnimation(cardPositionsAttack);
                break;
                case "BALANCE":
                cardPositions = cardPositionsBALANCE;
                CardAnimation.Instance.StartCardMoveAnimation(cardPositionsBALANCE);
                break;
            case "DEFENSE":
                cardPositions = cardPositionsDEFENSE;
                CardAnimation.Instance.StartCardMoveAnimation(cardPositionsDEFENSE);
                break;
        }

    }

    public void AddCard(GameObject cardObj)
    {
        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        if (cardPositions.Contains(rectTransform))
        {
            cardPositions.Remove(rectTransform);
        }

        Button buttonsword = cardObj.GetComponent<Button>();
        buttonsword.onClick.RemoveAllListeners();
        buttonsword.onClick.AddListener(() => PlayACard(cardObj));
        cardObj.transform.SetParent(attackParent);
        cards.Add(cardObj);
        switch (NumberBattleCards)
        {
            case 0:
                cards[NumberBattleCards].transform.localPosition = new Vector3(300f, -300f, 0f);
                break;
            case 1:
                cards[NumberBattleCards].transform.localPosition = new Vector3(0f, -300f, 0f);
                break;
            case 2:
                cards[NumberBattleCards].transform.localPosition = new Vector3(-300f, -300f, 0f);
                break;
        }      
        
        NumberBattleCards++;
        cardObj.SetActive(false);
        if (NumberBattleCards >= 3)
        {
            cards[0].SetActive(true);
            cards[1].SetActive(true);
            cards[2].SetActive(true);
            foreach(var rectran in cardPositions)
            {
                Button btn = rectran.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
            }
            UI4.SetActive(true);
            UI20.SetActive(false);
            UI21.SetActive(false);
            UI22.SetActive(false);
            

        }
        
    }

    public void ChooseACard(string name)
    {
        vlaue++;
        switch (name)
        {
            case "Axe":
                var axe = Instantiate(AXE, HandsPlayed);
                Button buttonaxe = axe.GetComponent<Button>();
                buttonaxe.onClick.AddListener(() => PlayACard(axe));
                cards.Add(axe);
                break;
            case "Bow":
                var bow = Instantiate(Bow, HandsPlayed);
                Button buttonbow = bow.GetComponent<Button>();
                buttonbow.onClick.AddListener(() => PlayACard(bow));
                cards.Add(bow);
                break;
            case "Lance":
                var lance = Instantiate(Lance, HandsPlayed);
                Button buttonlance = lance.GetComponent<Button>();
                buttonlance.onClick.AddListener(() => PlayACard(lance));
                cards.Add(lance);
                break;
            case "Shield":
                var shield = Instantiate(Shield, HandsPlayed);
                Button buttonshield = shield.GetComponent<Button>();
                buttonshield.onClick.AddListener(() => PlayACard(shield));
                cards.Add(shield);
                break;
            case "Sword":
                var sword = Instantiate(Sword, HandsPlayed);
                Button buttonsword = sword.GetComponent<Button>();
                buttonsword.onClick.AddListener(() => PlayACard(sword));
                cards.Add(sword);
                break;
            default:
            break;
        }
        if(vlaue >= 3)
        {
            cards[0].transform.position = new Vector3(300f, -300f, 0f);
            cards[1].transform.position = new Vector3(0f, -300f, 0f);
            cards[2].transform.position = new Vector3(-300f, -300f, 0f);
            UI4.SetActive(true);
            UI20.SetActive(false);
            UI21.SetActive(false);
            UI22.SetActive(false);
        }
    }


    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GameObject newObj = Instantiate(clickEffect);
            Vector3 mousePos = Input.mousePosition;
            newObj.transform.SetParent(this.transform, false);
            newObj.transform.position = mousePos;
            Destroy(newObj, 0.5f);
        }
    }


    public void PlayACard(GameObject obj)
    {

        PlayACardObject = obj;
        PlayACardObject.SetActive(true);
        foreach (var card in cards)
        {
			Vector3 originalPos = card.transform.localPosition;
			card.transform.localPosition = new Vector3(originalPos.x, -300f, originalPos.z);

		}
        obj.transform.position += new Vector3(0, 40, 0);
    }
    public void Play()
    {
        if (PlayACardObject == null) return;
        logInt--;
        logText.text = "Card Left:" + logInt.ToString();
        if (cards.Contains(PlayACardObject))
        {
            cards.Remove(PlayACardObject);
        } 
        StartCoroutine(FinishTheMove());
		
	}

    IEnumerator FinishTheMove()
    {
        Vector3 Pos = PlayACardObject.transform.position;

        CardAnimation.Instance.PlayCard(PlayACardObject.GetComponent<RectTransform>());
        yield return new WaitForSeconds(0.45f);
        if (cardPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, cardPositions.Count);
            RectTransform randomCardRect = cardPositions[randomIndex];
            GameObject randomCardObj = randomCardRect.gameObject;
            Button buttonsword = randomCardObj.GetComponent<Button>();
            buttonsword.onClick.AddListener(() => PlayACard(randomCardObj));
            randomCardObj.transform.SetParent(attackParent);
            randomCardObj.transform.position = Pos;
            randomCardObj.SetActive(true);
            if (cardPositions.Contains(randomCardRect))
            {
                cardPositions.Remove(randomCardRect);
            }
            cards.Add(randomCardObj);
			foreach (var card in cards)
			{
				Vector3 originalPos = card.transform.localPosition;
				card.transform.localPosition = new Vector3(originalPos.x, -300f, originalPos.z);

			}
		}
        Destroy(PlayACardObject);
        PlayACardObject = null;
    }
}
