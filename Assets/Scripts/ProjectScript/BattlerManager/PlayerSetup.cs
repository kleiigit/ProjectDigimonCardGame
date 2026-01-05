using ProjectScript.Enums;
using SinuousProductions;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public PlayerSide setPlayer;
    public bool isActivePlayer;
    public bool refill;

    public int maxLevelPartner;
    public int maxMemory;
    public int currentMemory;
    public GameObject checkzone;

    public List<DigimonDisplay> allDigimon = new();
    public List<Card> playerDeck = new();

    public List<GameObject> listPartnerObj = new();
    public List<GameObject> listHandObj = new();
    public List<GameObject> listEvoObj = new();
    public List<GameObject> listSecurityObj = new();
    public List<Card> listDiscardCards = new();
    public List<GameObject> listDataObj = new();

    [HideInInspector] public PartnerPileManager partnerPile;
    [HideInInspector] public HandManager hand;
    [HideInInspector] public EvoPileManager evoPile;
    [HideInInspector] public DrawPileManager drawPile;
    [HideInInspector] public SecurityPileManager securityPile;
    [HideInInspector] public DiscardManager discard;
    [HideInInspector] public DataPileManager dataPile;

    private void Awake()
    {
        partnerPile = GetComponent<PartnerPileManager>();
        hand = GetComponent<HandManager>();
        evoPile = GetComponent<EvoPileManager>();
        drawPile = GetComponent<DrawPileManager>();
        securityPile = GetComponent<SecurityPileManager>();
        discard = GetComponent<DiscardManager>();
        dataPile = GetComponent<DataPileManager>();
    }
    private void Start()
    {
        playerDeck = DeckManager.deckMain[setPlayer]; // corrigir futuramente
        Utility.Shuffle(playerDeck);

        listHandObj.Clear();
        listEvoObj.Clear();
        listDiscardCards.Clear();
        listDataObj.Clear();
        listSecurityObj.Clear();
        listEvoObj.Clear();
    }

    private void Update()
    {
        if (setPlayer == BattlePhaseManager.currentPlayer) isActivePlayer = true;
        if (listEvoObj.Count > 0)
        {
            maxLevelPartner = evoPile.GetActivePartner()?.level ?? 0;
            maxMemory = evoPile.GetActivePartner()?.leaderMemory ?? 0;
        }
    }

    public override string ToString()
    {
        return setPlayer.ToString();
    }
}
