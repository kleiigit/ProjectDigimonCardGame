using SinuousProductions;
using UnityEngine;
using ProjectScript.Interfaces;

public class HandManager : MonoBehaviour, IPile
{
    public GameObject playerObject;
    public RectTransform handTransform;

    private Vector3 originalHandPositions;

    public float fanSpread = 5f;
    public float cardSpacing = 5f;
    public float verticalSpacing = 100f;
    public float moveSpeed = 8f;

    private PlayerSetup setup;
    private GameObject cardPrefab;

    void Awake()
    {
        setup = playerObject.GetComponent<PlayerSetup>();
        originalHandPositions = handTransform.anchoredPosition;
        cardPrefab = GameManager.cardPrefab;
    }

    public void AddCard(Card cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        newCard.name = cardData.cardName.ToUpper() + " - " + cardData.cardID;
        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
        cardDisplay.cardData = cardData;
        newCard.GetComponent<MenuCardManager>().handOwner = setup.setPlayer;
        cardDisplay.UpdateCardDisplay();
        setup.listHandObj.Add(newCard);
        UpdateVisuals();
    }

    public void RemoveCard(GameObject cardObject)
    {
        Debug.Log($"Removendo carta da mão {setup.setPlayer}: {cardObject.name}");
        bool removed = setup.listHandObj.Remove(cardObject);
        Debug.Log(removed ? "Carta removida com sucesso." : "Carta não encontrada na lista.");
        if (removed)
        {
            setup.listHandObj.RemoveAll(card => card == null);
            UpdateVisuals();
            Destroy(cardObject);
        }
    }

    public void UpdateVisuals()
    {
        setup.listHandObj.RemoveAll(card => card == null);

        int cardCount = setup.listHandObj.Count;
        if (cardCount == 0) return;

        if (cardCount == 1)
        {
            setup.listHandObj[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            setup.listHandObj[0].transform.localPosition = Vector3.zero;
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            setup.listHandObj[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));
            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            setup.listHandObj[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }
    public void HideHand()
    {
        handTransform.gameObject.SetActive(false);
    }
}
