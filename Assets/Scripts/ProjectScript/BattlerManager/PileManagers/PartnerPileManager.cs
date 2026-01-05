using ProjectScript.Enums;
using ProjectScript.Interfaces;
using SinuousProductions;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PartnerPileManager : MonoBehaviour, IPile
{
    private PlayerSetup setup;
    private ControlBattleField control;
    private GameObject cardPrefab;

    public Transform partnerPileTransform;
    public GameObject partnerPileZone;
    public float cardSpacing = 100f;

    //public static event Action<PlayerSide,Card> OnPartnerChosenHandler;

    void Awake()
    {
        control = FindFirstObjectByType<ControlBattleField>();
        setup = GetComponent<PlayerSetup>();
        cardPrefab = GameManager.cardPrefab;
    }

    void Start()
    {
        InitializePartnerPile(setup);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!GameManager.ClickedOnCard())
            {
                control.HideAllPartnerPiles();
                control.ShowHandForBoth();
            }
        }
    }
    public void AddCard(Card cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, partnerPileTransform.position, Quaternion.identity, partnerPileTransform);
        newCard.GetComponent<CardDisplay>().cardData = cardData;
        newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        newCard.GetComponent<MenuCardManager>().handOwner = setup.setPlayer;

        // 🔹 adiciona efeito de hover (vai para frente e volta)
        if (newCard.GetComponent<CardHoverEffect>() == null)
        {
            newCard.AddComponent<CardHoverEffect>();
        }

        setup.listPartnerObj.Add(newCard);
        UpdateVisuals();
        newCard.name = cardData.cardName.ToUpper() + " - " + cardData.cardID;
    }

    public void RemoveCard(GameObject cardObject)
    {
        bool removed = setup.listPartnerObj.Remove(cardObject);

        if (removed)
        {
            setup.listPartnerObj.RemoveAll(card => card == null);
            UpdateVisuals();
            Destroy(cardObject);
        }
    }

    public void UpdateVisuals()
    {
        setup.listPartnerObj.RemoveAll(card => card == null);

        for (int i = 0; i < setup.listPartnerObj.Count; i++)
        {
            float x = i * cardSpacing;
            setup.listPartnerObj[i].transform.localPosition = new Vector3(x, 0, 0);
            setup.listPartnerObj[i].transform.localRotation = Quaternion.identity;
        }
    }

    private static void InitializePartnerPile(PlayerSetup setup)
    {
        // colocar aqui o deck padrão caso deckPatner estiver vazio
        foreach (Card card in DeckManager.deckPartner[setup.setPlayer])
        {
            setup.partnerPile.AddCard(card);
        }
    }

    public void HidePartnerPile()
    {
        control.HideAllPartnerPiles();
        control.ShowHandForBoth();
    }

    
    // Button OnClick para o PartnerPile

    public void UpdatePartnerPileCostColors()
    {
        foreach (var cardGO in setup.listPartnerObj)
        {
            if (cardGO == null) continue;

            var cardDisplay = cardGO.GetComponent<CardDisplay>();
            if (cardDisplay == null)
            {
                Debug.LogWarning($"Carta no PartnerPile do lado {setup.setPlayer} sem CardDisplay.");
                continue;
            }

            bool canPlay = setup.dataPile.HasSufficientDataToPlayCard(cardDisplay.cardData.GetColorCost());
        }
    }
    public void ShowPartnerPile()
    {
        partnerPileTransform.gameObject.SetActive(true);
        setup.hand.HideHand();
    }
    public void OpenSelectionUI(int levelFilter)
    {
        var filteredCards = setup.listPartnerObj.Where(cardGO =>
            {
                var cardDisplay = cardGO.GetComponent<CardDisplay>();
                if (cardDisplay == null) return false;

                DigimonCard digimon = cardDisplay.cardData as DigimonCard;
                return digimon != null && digimon.level == levelFilter;
            })
            .ToList();

        if (filteredCards.Count == 0)
        {
            Debug.LogWarning($"Nenhum Digimon nível {levelFilter} disponível para {setup.setPlayer}.");
            return;
        }

        partnerPileTransform.gameObject.SetActive(true);
        setup.hand.HideHand();
    }
}

/// <summary>
/// Script simples para enviar a carta para frente quando o mouse passa por cima.
/// </summary>
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int originalSiblingIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling(); // vai para frente
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.SetSiblingIndex(originalSiblingIndex); // volta para posição original
    }
}
