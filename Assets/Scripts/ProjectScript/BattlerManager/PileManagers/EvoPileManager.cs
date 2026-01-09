
using ProjectScript.Enums;
using ProjectScript.Interfaces;
using SinuousProductions;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EvoPileManager : MonoBehaviour, IPile
{
    private PlayerSetup setup;
    private GameObject cardPrefab;
    [SerializeField]
    private GameObject activePartner;
    private ControlBattleField control;
    private GridCell evoCell;

    public RectTransform rectEvoPile;
    public GameObject evoZone;
    public float cardSpacing = 100f;

    public static event Action<PlayerSide, Card> OnPartnerChosen;
    private void Awake()
    {
        setup = GetComponent<PlayerSetup>();
        control = FindFirstObjectByType<ControlBattleField>();
        cardPrefab = GameManager.cardPrefab;
        evoCell = evoZone.GetComponent<GridCell>();
    }
    private void Update()
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
        if(rectEvoPile == null)
        {
            Debug.LogError("Evo Trastorm Transform is not assigned.");
            return;
        }
        if(cardData == null)
        {
            Debug.LogError("Card Data is null. Cannot add card to Evo Pile.");
            return;
        }
        if(cardPrefab == null)
        {
            Debug.LogError("Card Prefab is not assigned in GameManager.");
            return;
        }
        GameObject newCard = Instantiate(cardPrefab, rectEvoPile.position, Quaternion.identity, rectEvoPile);
        newCard.GetComponent<CardDisplay>().cardData = cardData;
        newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        newCard.GetComponent<MenuCardManager>().handOwner = setup.setPlayer;
        newCard.name = cardData.cardName.ToUpper() + " - " + cardData.cardID;
        UpdateVisuals();

        //evo
        setup.listEvoObj.Insert(0, newCard);
        GameObject topEvoCard = Instantiate(newCard, evoZone.transform.position, Quaternion.identity, evoZone.transform);
        topEvoCard.GetComponent<MenuCardManager>();
        MenuCardManager menuCard = topEvoCard.GetComponent<MenuCardManager>();
        menuCard.handOwner = setup.setPlayer;

        //grid cell
        if(evoCell.owner != setup.setPlayer)
        {
            Debug.LogError("Evo Cell owner does not match player setup");
            return;
        }
        menuCard.currentCell = evoCell;
        evoCell.ObjectInCell = topEvoCard;
        evoCell.cellFull = true;
        topEvoCard.layer = 11;

        // transform
        Transform transform = topEvoCard.transform;
        transform.localScale = new(0.57f, 0.55f, 0f);
        if(setup.setPlayer == PlayerSide.PlayerRed) transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
        transform.transform.position = new Vector3(evoZone.transform.position.x, evoZone.transform.position.y, 0);
        topEvoCard.name = "Active Partner - " + cardData.cardName.ToUpper() + " - " + cardData.cardID;
        Destroy(activePartner);
        activePartner = topEvoCard;
        activePartner.GetComponent<FieldCard>().enabled = true;
        activePartner.GetComponent<DigimonDisplay>().enabled = true;

        OnPartnerChosen?.Invoke(setup.setPlayer, cardData);
    }
    public void RemoveCard(GameObject cardObject)
    {
        int index = setup.listEvoObj.IndexOf(cardObject);
        bool removed = setup.listEvoObj.Remove(cardObject);

        if (removed)
        {
            setup.listPartnerObj.RemoveAll(card => card == null);
            UpdateVisuals();
            Destroy(cardObject);
            if(index == 0)
            {
                evoCell.ObjectInCell = null;
                evoCell.cellFull = false;
            }
        }
    }
    public void UpdateVisuals()
    {
        setup.listPartnerObj.RemoveAll(card => card == null);

        for (int i = 0; i < setup.listPartnerObj.Count; i++)
        {
            float x = i * cardSpacing;
            setup.listPartnerObj[i].transform.
                SetLocalPositionAndRotation(new Vector3(x, 0, 0), Quaternion.identity);
        }
    }

    public DigimonCard? GetActivePartner()
    {
        if (setup.listEvoObj.Count > 0)
        {
            if (activePartner == null) return null;
            Card activeCard = activePartner.GetComponent<CardDisplay>().cardData;
            return activeCard as DigimonCard;
        }
        return null;
    }
    public void ShowEvoPile()
    {
        rectEvoPile.gameObject.SetActive(true);
    }

    public bool CanEvolveCard(Card partner)
    {
        DigimonCard digiPartner = partner as DigimonCard;
        if (setup.dataPile.HasSufficientDataToPlayCard(partner.GetColorCost()))
        {
            if (activePartner == null)
            {
                if (digiPartner.level > 0)
                {
                    Debug.LogWarning("noButton active partner to evolve from.");
                    return false;
                }

                else if (digiPartner.level == 0)
                {
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot evolve to this partner. Level requirement not met.");
                    return false;
                }
            }
            else
            {
                if (BattlePhaseManager.phase == 0) return false;

                if ((digiPartner.level -1 == (activePartner.GetComponent<CardDisplay>().cardData as DigimonCard).level))
                {
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot evolve to this partner. Level requirement not met.");
                    return false;
                }
            }
        }
        else
        {
            Debug.Log("Data insuficiente");
            return false;
        }
    }
}
