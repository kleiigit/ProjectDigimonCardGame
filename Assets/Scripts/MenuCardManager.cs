using ProjectScript.Enums;
using SinuousProductions;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuCardManager : MonoBehaviour
{
    [Header("Card Controller Setup")]
    public PlayerSide handOwner;

    // --- Referências de managers ---
    private DataPileManager dataPileManager;
    private EffectManager effectManager;
    private CardDisplay cardDisplay;
    private HandManager handManager;
    private FieldCard fieldCard;
    private CardPlaySelector cardPlaySelector;
    private PartnerPileManager partnerPileManager;
    private ControlBattleField control;

    private PlayerSetup setup;

    public GridCell currentCell;
    public Button buttonInteractiveCard;

    private void Awake()
    {
        dataPileManager = FindFirstObjectByType<DataPileManager>();
        handManager = FindFirstObjectByType<HandManager>();
        partnerPileManager = FindFirstObjectByType<PartnerPileManager>();
        control = FindFirstObjectByType<ControlBattleField>();
        effectManager = FindFirstObjectByType<EffectManager>();

        fieldCard = GetComponent<FieldCard>();
        cardDisplay = GetComponent<CardDisplay>();
        cardPlaySelector = GetComponent<CardPlaySelector>();
        
    }
    private void Start()
    {
        if(handOwner == PlayerSide.PlayerBlue)
        {
            setup = GameSetupStart.playerBlue;
        }
        else
        {
            setup = GameSetupStart.playerRed;
        }
    }

    public void ButtonInteractive()
    {
        // Deck edit não abre menu
        if (GameManager.isInDeckEditScreen)
        {
            // Add and Remove cards from deck
            EditDeckFunc();
            return;
        }

        // Bloqueios
        if (GetComponentInParent<GridLayoutGroup>() != null) return;
        if (gameObject.layer == LayerMask.NameToLayer("Data")) return;

        // Sempre fecha menu anterior
        CardContextMenu globalMenu = FindFirstObjectByType<CardContextMenu>();
        if (globalMenu == null) return;

        List<CardContextMenu.MenuOption> options = new List<CardContextMenu.MenuOption>();

        // Inspecionar (sempre disponível)
        options.Add(new CardContextMenu.MenuOption("Inspect", () =>
        {
            Debug.Log($"Inspecionar {cardDisplay.cardName.text}");
            // Aqui você pode abrir uma tela detalhada da carta
        }));

        // Define opções por fase
        switch (BattlePhaseManager.phase)
        {
            case 0: // Setup
                if (cardDisplay.cardData.cardType == CardType.Partner)
                {
                    options.Add(new CardContextMenu.MenuOption("Choose Partner", ButtonEvoPartner));
                }
                break;

            case Phase.CostPhase: // Cost
                options.Add(new CardContextMenu.MenuOption("Send to Data", ButtonCostPhaseConfirm));
                break;

            case Phase.EvolutionPhase: // Evolution
                options.Add(new CardContextMenu.MenuOption("Evolve Partner", ButtonEvoPartner));
                break;

            case Phase.MainPhase: // Main
                options.Add(new CardContextMenu.MenuOption("Play Card", ButtonPlayCard));
                options.Add(new CardContextMenu.MenuOption("Attack", ButtonAttack));
                options.Add(new CardContextMenu.MenuOption("Activate Effect", ButtonEffectCard));
                break;

            case Phase.BattlePhase: // Battle
            case Phase.AttackPhase: // Attack
                options.Add(new CardContextMenu.MenuOption("Attack", ButtonAttack));
                break;
        }

        // Chama o menu
        globalMenu.ShowMenu(this.gameObject, options);
        buttonInteractiveCard.interactable = false;
    }

    // EDICK DECK MENU
    public void EditDeckFunc()
    {
        DeckEditorUI editor = FindFirstObjectByType<DeckEditorUI>();
        if (editor != null)
        {

            // Acessar o objeto filho "CardCounter"
            TextMeshProUGUI counterText = GetComponentsInChildren<TextMeshProUGUI>(true)
                .FirstOrDefault(t => t.gameObject.name == "CounterText");

            if (counterText != null)
            {

                string rawText = counterText.text.Trim().ToLower();
                if (rawText.StartsWith("x") && int.TryParse(rawText.Substring(1), out int currentValue))
                {
                    if (currentValue > 0)
                    {
                        if (cardDisplay == null || cardDisplay.cardData == null)
                        {
                            Debug.LogError("[ButtonPhaseCard] 'cardDisplay' ou 'cardData' está nulo. GameObject: " + gameObject.name);
                            return;
                        }
                        Card cardBase = cardDisplay.cardData;
                        bool added = editor.AddCardToEditingDeck(cardBase, ref currentValue);

                        if (added)
                        {
                            counterText.text = "x" + currentValue;
                        }
                    }
                }
            }
            else
            {

                GameObject cardT = editor.RemoveCardDeck(cardDisplay.cardData);

                if (cardT != null)
                {

                    // Procura o TextMeshProUGUI filho com nome "CounterText"
                    TextMeshProUGUI counterTText = cardT.GetComponentsInChildren<TextMeshProUGUI>(true)
                        .FirstOrDefault(t => t.gameObject.name == "CounterText");

                    if (counterTText != null)
                    {

                        string rawText = counterTText.text.Trim().ToLower();
                        if (rawText.StartsWith("x") && int.TryParse(rawText.Substring(1), out int currentValue))
                        {

                            currentValue += 1; // Adiciona +1 ao contador
                            counterTText.text = "x" + currentValue;


                            Destroy(this.gameObject);
                        }
                    }
                    else { Debug.Log("nao foi encontrado contador"); }
                }


                else { Debug.Log("CardT nao encontrado"); }
            }
        }
        else
        {
            Debug.LogWarning("DeckEditorUI nao encontrado na cena.");
        }

        return;
    }
    // -------------------------
    // AÇÕES DO MENU
    // -------------------------

    public void ButtonEffectCard()
    {
        Card cardBase = cardDisplay.cardData;
        if (cardBase.effects.Count > 0)
        {
            effectManager.ExecuteCardEffect(cardBase.effects[0], cardBase);
        }
    }
    public void ButtonAttack()
    {
        currentCell = fieldCard.parentCell;

        if ((int)BattlePhaseManager.phase == 6)
        {
            fieldCard.SetDownPosition();
            var cardData = fieldCard.digimonDisplay;
            if (cardData != null && cardData.cardType == CardType.Partner)
            {
                BattlePhaseManager.phase = Phase.EndPhase;
            }
            control.GetOpponentAtFront(currentCell, handOwner, this.gameObject);
        }
        else if ((int)BattlePhaseManager.phase == 5 || (int)BattlePhaseManager.phase == 4)
        {
            BattlePhaseManager.phase++;
        }
    }
    public void ButtonEvoPartner()
    {
        DigimonCard cardPartner = cardDisplay.cardData as DigimonCard;
        if(setup == null)
        {
            Debug.LogWarning("PlayerSetup não encontrado para o lado: " + handOwner);
            setup = control.GetPlayerSetup(handOwner);
        }
        DigimonCard cardActivePartner = setup.evoPile.GetActivePartner();
        if (dataPileManager.HasSufficientDataToPlayCard(cardPartner, handOwner))
        {
            if (cardActivePartner == null)
            {
                if (cardPartner.level > 0)
                {
                    Debug.LogWarning("No active partner to evolve from.");
                    return;
                }
                
                if (cardPartner.level != 0)
                {
                    Debug.LogWarning("Cannot evolve to this partner. Level requirement not met.");
                    return;
                }
            }
            else if (!(cardActivePartner.level == cardPartner.level - 1))
            {
                Debug.LogWarning("Cannot evolve to this partner. Level requirement not met.");
                return;
            }
            // colocar a logica de evolução em um local apropriado
            //setup.partnerPile.ChoosePartner(this.gameObject);
            Debug.Log($"Evolving partner to {cardPartner.cardName} - {setup.setPlayer}");
            setup.evoPile.AddCard(cardPartner);
            setup.partnerPile.RemoveCard(this.gameObject);
            Destroy(gameObject);
            partnerPileManager.HidePartnerPile();

            if ((int)BattlePhaseManager.phase > 1)
                BattlePhaseManager.phase++;
        }
        else
        {
            Debug.LogWarning("Not enough data to evolve partner.");
        }
    }
    public void ButtonPlayCard()
    {
        DigimonCard cardDigimon = cardDisplay.cardData as DigimonCard;
        if (cardDigimon != null && cardDigimon.cardType == CardType.Digimon)
        {
            int topLevel = control.GetTopLevel(handOwner);
            int currentMemory = control.GetTotalLevel(handOwner);
            int topMemory = control.GetTopMemory(handOwner);

            int levelCardToPlay = cardDigimon.level;
            int sumWithNewCard = currentMemory + levelCardToPlay;

            if (levelCardToPlay <= topLevel && sumWithNewCard <= topMemory)
            {
                cardPlaySelector.StartCardPlacement();
            }
        }
        else if (cardDisplay.cardData.cardType == CardType.Program)
        {
            if (dataPileManager.HasSufficientDataToPlayCard(cardDisplay.cardData, handOwner))
            {
                Debug.Log("Program card played.");
            }
        }
    }
    public void ButtonCostPhaseConfirm()
    {
        PlayerSide sidePlay = (BattlePhaseManager.currentPlayer == PlayerSide.PlayerBlue) ? PlayerSide.PlayerBlue : PlayerSide.PlayerRed;
        Card cardBase = cardDisplay.cardData;
        if (cardBase.cardType == CardType.Digimon || cardBase.cardType == CardType.Program)
        {
            setup.dataPile.MoveCardFromHandToDataPile(this.gameObject);
            BattlePhaseManager.phase++;
        }
    }
}
