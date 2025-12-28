using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SinuousProductions;
using ProjectScript.Enums;
using ProjectScript.Interfaces;

public class DiscardManager : MonoBehaviour, IPile
{
    public Transform discardGrid;
    [SerializeField]
    private GameObject topDiscardCard;

    [SerializeField] private Vector3 topCardScale = new Vector3(0.85f, 0.85f, 1f);

    DisplayListCards displayListCards;
    PlayerSetup setup;

    private void Awake()
    {
        displayListCards = FindFirstObjectByType<DisplayListCards>();
        setup = GetComponent<PlayerSetup>();
    }

    public void AddCard(Card card)
    {
        GameObject newCard = Instantiate(GameManager.cardPrefab, discardGrid);
        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
        cardDisplay.cardData = card;
        cardDisplay.UpdateCardDisplay();

        newCard.transform.localScale = Vector3.one;
        newCard.name = card.cardName;
        newCard.layer = 15; //trash

        setup.listDiscardObj.Add(newCard);

        // Atualiza o visual do topo para esta nova carta
        newCard.transform.SetAsLastSibling();
        newCard.transform.localScale = topCardScale;
        UpdateVisuals();
    }
    public void RemoveCard(GameObject cardObject)
    {
        Destroy(cardObject);
    }

    public List<GameObject> PullAllFromDiscard(PlayerSide side)
    {
        List<GameObject> cardsToReturn = new List<GameObject>(setup.listDiscardObj);
        setup.listDiscardObj.Clear();
        UpdateVisuals();

        return cardsToReturn;
    }

    public void UpdateVisuals()
    {
        if (setup.listDiscardObj.Count == 0) return;
        // Se não houver cartas, destrói o visual do topo (se existir)
        if (topDiscardCard != null)
        {
            Destroy(topDiscardCard);
            topDiscardCard = null;
        }

        GameObject topCard = setup.listDiscardObj[^1];

        if (topDiscardCard != topCard)
        {
            // Se o visual do topo for diferente da última carta da lista, ajusta:
            if (topDiscardCard != null)
            {
                Destroy(topDiscardCard);
            }

            topDiscardCard = topCard;
        }

        topCard.transform.SetAsLastSibling();
        topCard.transform.localScale = topCardScale;
    }

    public void ListDiscardCardsButton(PlayerSide side)
    {
        string listDescription = $"Cartas na pilha de descarte do jogador {side}";
        Debug.Log("Botão de lista acionado");

        List<Card> cardDataList = new();

        foreach (var cardGO in setup.listDiscardObj)
        {
            if (cardGO == null || !cardGO) continue;

            CardDisplay display = cardGO.GetComponent<CardDisplay>();
            if (display != null && display.cardData != null)
            {
                cardDataList.Add(display.cardData);
            }
        }

        if (cardDataList.Count == 0)
        {
            Debug.LogWarning("[DiscardClickHandler] Nenhuma carta com dados válidos para exibir.");
            return;
        }

        DisplayListCards.Instance.ShowCardList(cardDataList, listDescription, false);
    }

}
