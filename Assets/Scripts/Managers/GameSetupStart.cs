using ProjectScript.Enums;
using SinuousProductions;
using UnityEngine;

public class GameSetupStart : MonoBehaviour
{
    public int startingHandSize = 5;
    public int drawPerTurn = 2;
    public int securityPileSize = 7;
    public int maxEndTurnSize = 6;

    // player Setup
    [SerializeField] private PlayerSetup playerBlueSetup;
    [SerializeField] private PlayerSetup playerRedSetup;
    public static PlayerSetup playerBlue;
    public static PlayerSetup playerRed;  

    public bool playerBlueChosen = false;
    public bool playerRedChosen = false;
    public bool setupCompleted = false;

    private void Awake()
    {
        playerBlue = playerBlueSetup;
        playerRed = playerRedSetup;
    }
    void Start()
    {
        EvoPileManager.OnPartnerChosen += OnPartnerChosenHandler;

        playerBlueSetup.partnerPile.OpenSelectionUI(0);
        playerRedSetup.partnerPile.OpenSelectionUI(0);
    }

    private void OnDestroy()
    {
        EvoPileManager.OnPartnerChosen -= OnPartnerChosenHandler;
    }

    private void OnPartnerChosenHandler(PlayerSide playerSide, Card chosenPartner)
    {
       

        if (playerSide == PlayerSide.PlayerBlue) playerBlueChosen = true;
        else if (playerSide == PlayerSide.PlayerRed) playerRedChosen = true;

        if (playerBlueChosen && playerRedChosen && !setupCompleted)
        {
            setupCompleted = true;
            FirstHand();
        }
    }

    private void FirstHand()
    {
        
        DrawPileManager.BattleSetupHand(startingHandSize);

        Debug.LogWarning($"[GameSetupStart] Pile Blue tem {playerBlueSetup.playerDeck.Count} cartas após compra inicial.");
        Debug.LogWarning($"[GameSetupStart] Pile Red tem {playerRedSetup.playerDeck.Count} cartas após compra inicial.");

        SecurityPileManager.BattleSetupSecurity(securityPileSize);

        BattlePhaseManager.roundCount = 1;
        Debug.Log("[GameSetupStart] Setup concluído, batalha iniciada.");
    }
}
