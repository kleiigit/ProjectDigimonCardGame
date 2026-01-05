using ProjectScript.Enums;
using SinuousProductions;
using UnityEngine;

public class GameSetupStart : MonoBehaviour
{
    public static PlayerSetup playerBlue;
    public static PlayerSetup playerRed;

    public static int endTurnSize;

    public int startingHandSize = 5;
    public int drawPerTurn = 2;
    public int securityPileSize = 7;
    public int maxEndTurnSize = 6;

    // player Setup
    [SerializeField] private PlayerSetup playerBlueSetup;
    [SerializeField] private PlayerSetup playerRedSetup;
     

    public bool playerBlueChosen = false;
    public bool playerRedChosen = false;
    public bool setupCompleted = false;

    private void Awake()
    {
        playerBlue = playerBlueSetup;
        playerRed = playerRedSetup;
        endTurnSize = maxEndTurnSize;
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

    public static PlayerSetup GetPlayerSetup(PlayerSide player)
    {
        if (playerBlue == null || playerRed == null)
        {
            Debug.LogWarning("setup nao encontrado");
        }
        if (player == PlayerSide.PlayerBlue)
            return playerBlue;
        else
            return playerRed;
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
        SecurityPileManager.BattleSetupSecurity(securityPileSize);

        BattlePhaseManager.roundCount = 1;
        Debug.LogWarning("[GameSetupStart] Setup concluído, batalha iniciada.");
    }
}
