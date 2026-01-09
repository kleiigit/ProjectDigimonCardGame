using System.Collections.Generic;
using UnityEngine;
using SinuousProductions;
using ProjectScript.Enums;
using ProjectScript.Interfaces;

public class DiscardManager : MonoBehaviour, IPile
{
    public Transform discardGrid;
    [SerializeField]
    private GameObject topDiscardCard;
    [SerializeField] private GameObject discardCardsParent;

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
        if(topDiscardCard != null)
            Destroy(topDiscardCard);

        GameObject newCard = Instantiate(GameManager.cardPrefab, discardGrid);
        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
        cardDisplay.cardData = card;
        cardDisplay.UpdateCardDisplay();

        newCard.transform.localScale = Vector3.one;
        newCard.name = card.cardName;
        newCard.layer = 17; //trash
        newCard.GetComponent<MenuCardManager>().handOwner = setup.setPlayer;
        topDiscardCard = newCard;
        setup.listDiscardCards.Add(card);

        // Atualiza o visual do topo para esta nova carta
        newCard.transform.SetAsLastSibling();
        newCard.transform.localScale = topCardScale;
        TriggerCardManager.TriggerDiscarted();
        UpdateVisuals();
    }
    public void RemoveCard(GameObject cardObject)
    {
        Destroy(cardObject);
    }

    public List<Card> PullAllFromDiscard(PlayerSide side)
    {
        setup.listDiscardCards.Clear();
        UpdateVisuals();

        return setup.listDiscardCards;
    }

    public void UpdateVisuals()
    {
        if (setup.listDiscardCards.Count == 0 || topDiscardCard == null) return;
        topDiscardCard.layer = 17;
        topDiscardCard.transform.SetAsLastSibling();
        topDiscardCard.transform.localScale = topCardScale;
    }

    public void ListDiscardCardsButton()
    {
        if (setup == null)
        {
            Debug.LogError("Setup não foi inicializado no DiscardManager.");
            return;
        }
        if (setup.listDiscardCards == null)
        {
            Debug.LogError("listDiscardCards não foi inicializada.");
            return;
        }
        if (setup.listDiscardCards.Count == 0)
        {
            Debug.LogWarning("Nenhuma carta para exibir.");
            return;
        }

        string listDescription = $"Lixeira do {setup.evoPile.GetActivePartner().cardName}";
        Debug.Log("Botão de lista acionado");

        displayListCards.side = setup.setPlayer;

        // necessário configuração de posse para saber se o jogador pode ativar ou nao efeitos das cartas na lixeira
        displayListCards.isOwner = false;
        LayerMask layerMask = 15;

        displayListCards.ShowCardList(setup.listDiscardCards, layerMask, listDescription, false);
    }


}
