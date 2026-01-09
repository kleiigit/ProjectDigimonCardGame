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
    private FieldCard fieldCard;
    private CardPlaySelector cardPlaySelector;
    private ControlBattleField control;
    Card _cardData;
    private PlayerSetup setup;

    public GridCell currentCell;
    public Button buttonInteractiveCard;

    private void Awake()
    {
        control = FindFirstObjectByType<ControlBattleField>();
        fieldCard = GetComponent<FieldCard>();
        cardPlaySelector = GetComponent<CardPlaySelector>();
        
    }
    private void Start()
    {
        _cardData = GetComponent<CardDisplay>().cardData;
        if (handOwner == PlayerSide.PlayerBlue)
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
        // Deck Edit
        if (GameManager.isInDeckEditScreen)
        {
            EditDeckFunc();
            return;
        }

        // Bloqueios gerais
        if (GetComponentInParent<GridLayoutGroup>() != null) return;

        if (gameObject.layer == LayerMask.NameToLayer("DataPile"))
        {
            setup.dataPile.ListDataCardsButton();
            return;
        }
        if (gameObject.layer == 17)
        {
            setup.discard.ListDiscardCardsButton();
            return;
        }
        
        Card _cardData = GetComponent<CardDisplay>().cardData;
        CardContextMenu globalMenu = FindFirstObjectByType<CardContextMenu>();
        if (globalMenu == null) return;

        List<CardContextMenu.MenuOption> options = new();

        CardType cardType = _cardData.cardType;
        Phase currentPhase = BattlePhaseManager.phase;
        switch (currentPhase)
        {
            case Phase.UpPhase:
                if (_cardData.cardType == CardType.Partner && setup.evoPile.CanEvolveCard(_cardData))
                {
                    options.Add(new CardContextMenu.MenuOption(
                        "Evolve Partner",
                        ButtonEvoPartner));
                }
                break;

            case Phase.CostPhase:
                if (gameObject.layer == 16 || gameObject.layer == 7)
                {
                    options.Add(new CardContextMenu.MenuOption(
                        "Send to Data",
                        ButtonCostPhaseConfirm
                    ));
                }
                break;

            case Phase.EvolutionPhase:
                if (setup.evoPile.CanEvolveCard(_cardData))
                {
                    options.Add(new CardContextMenu.MenuOption(
                        "Evolve Partner",
                        ButtonEvoPartner
                    ));
                }
                break;

            case Phase.MainPhase:
                HandleMainPhase(options, cardType);
                break;

            case Phase.PreparationPhase:
                HandleSkillPhase(options, cardType);
                break;

            case Phase.AttackPhase:
                if (gameObject.layer == 7 || gameObject.layer == 11)
                {
                    if (GetComponent<FieldCard>().downPosition == false)
                    {
                        options.Add(new CardContextMenu.MenuOption(
                        "Attack",
                        ButtonAttack));
                    }
                    // Keyword para configura quando houver necessidade.
                    //if (GetComponent<CardDisplay>().cardData.effects.Count > 0)
                    //{ options.Add(new CardContextMenu.MenuOption("Activate Effect",ButtonEffectCard));} 
                }
                break;
        }
        if (options.Count == 0) return;
        globalMenu.ShowMenu(gameObject, options);
        buttonInteractiveCard.interactable = false;
    }
    private void HandleMainPhase(List<CardContextMenu.MenuOption> options,CardType cardType)
    {
        if (gameObject.layer == 7 || gameObject.layer == 11)
        {
            if (BattlePhaseManager.roundCount != 1)
            {
                if (GetComponent<FieldCard>().downPosition == false)
                {
                    options.Add(new CardContextMenu.MenuOption(
                    "Attack",
                    ButtonAttack));
                }
            }

            foreach (CardEffects item in GetComponent<CardDisplay>().cardData.effects.
                Where(p => p.trigger == CardEffects.Trigger.Action))
            {
                if(EffectManager.CriteriaEffect(item, _cardData, setup))
                {
                    options.Add(new CardContextMenu.MenuOption(
                        "Activate Effect",
                        ButtonEffectCard));
                }
            }
        }

        if (cardType == CardType.Skill || gameObject.layer == 10)
        {
            Card.SkillActivation timing =
                (_cardData as ProgramandSkillCard).skillTimeActivation;

            if (timing != Card.SkillActivation.MainPhase &&
                timing != Card.SkillActivation.MainPhaseAndBattlePhase)
                return;
            if (setup.dataPile.HasSufficientDataToPlayCard(_cardData.GetColorCost()))
            {

            }
            else
            {
                Debug.Log("Data insuficiente");
                return;
            }
        }
        else if (cardType == CardType.Digimon)
        {
            if (!(_cardData as DigimonCard).CanDigimonPlayed(setup))
                return;

            if (gameObject.layer == 7)
                return;
        }
        else if (cardType == CardType.Program)
        {
            if(!setup.dataPile.HasSufficientDataToPlayCard(_cardData.GetColorCost()))
            {
                Debug.Log("Data insuficiente");
                return;
            }

        }
        else
        {
            return;
        }

        options.Add(new CardContextMenu.MenuOption(
            "Play Card",
            ButtonPlayCard
        ));
    }
    private void HandleSkillPhase(List<CardContextMenu.MenuOption> options, CardType cardType)
    {
        if (cardType != CardType.Skill) return;

        if ((_cardData as ProgramandSkillCard).skillTimeActivation == Card.SkillActivation.AttackPhase)
        {
            options.Add(new CardContextMenu.MenuOption("Play Card", ButtonPlayCard));
        }
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
                        if (_cardData == null || _cardData == null)
                        {
                            Debug.LogError("[ButtonPhaseCard] 'card' ou 'cardData' está nulo. GameObject: " + gameObject.name);
                            return;
                        }
                        Card cardBase = _cardData;
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

                GameObject cardT = editor.RemoveCardDeck(_cardData);

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
        if (_cardData.effects.Count > 0)
        {
            TriggerCardManager.TriggerActivateAction(_cardData, setup);
        }
    }
    public void ButtonAttack()
    {
        currentCell = fieldCard.parentCell;
        // teste
        BattlePhaseManager.phase = Phase.AttackPhase;

        if (BattlePhaseManager.phase == Phase.AttackPhase)
        {
            fieldCard.SetDownPosition();
            var cardData = fieldCard.digimonDisplay;
            if (cardData != null && cardData.cardType == CardType.Partner)
            {
                BattlePhaseManager.phase = Phase.EndPhase;
            }
            control.GetOpponentAtFront(currentCell, handOwner, gameObject);

        }
        else if (BattlePhaseManager.phase == Phase.MainPhase)
        {
            BattlePhaseManager.phase = Phase.PreparationPhase;
        }
    }
    public void ButtonEvoPartner()
    {
        DigimonCard cardPartner = _cardData as DigimonCard;

        SelectionDataManager.CostCard(setup, cardPartner.GetColorCost(),
            () =>
            {
                Debug.Log($"Evolving partner to {cardPartner.cardName} - {setup.setPlayer}");

                setup.evoPile.AddCard(cardPartner);
                setup.partnerPile.RemoveCard(this.gameObject);
                Destroy(gameObject);
                setup.partnerPile.HidePartnerPile();

                if ((int)BattlePhaseManager.phase > 1)
                    BattlePhaseManager.phase++;
            }
        );
    }

    public void ButtonPlayCard()
    {
        DigimonCard cardDigimon = _cardData as DigimonCard;
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
        else {
            SelectionDataManager.CostCard(setup, _cardData.GetColorCost(),
            () =>
            {
                if (_cardData.cardType == CardType.Program)
                {
                    if (setup.dataPile.HasSufficientDataToPlayCard(_cardData.GetColorCost()))
                    {
                        Debug.Log("Program card played.");
                        UIWindowManager.Instance.MoveToCheckZone(_cardData, setup, FieldPlace.Hand);
                        setup.hand.RemoveCard(this.gameObject);
                    }
                }
                else if (_cardData.cardType == CardType.Skill)
                {
                    if (setup.dataPile.HasSufficientDataToPlayCard(_cardData.GetColorCost()))
                    {
                        Debug.Log("Skill card played.");
                        setup.partnerPile.HidePartnerPile();
                        UIWindowManager.Instance.MoveToCheckZone(_cardData, setup, FieldPlace.PartnerPile);
                        setup.partnerPile.RemoveCard(this.gameObject);
                    }
                }
            }
            );
        }
        
    }
    public void ButtonCostPhaseConfirm()
    {
        PlayerSide sidePlay = (BattlePhaseManager.currentPlayer == PlayerSide.PlayerBlue) ? PlayerSide.PlayerBlue : PlayerSide.PlayerRed;
        Card cardBase = _cardData;
        if (cardBase.cardType == CardType.Digimon || cardBase.cardType == CardType.Program)
        {
            setup.dataPile.MoveCardFromHandToDataPile(gameObject);
            if (gameObject.layer == 7)
            {
                GetComponent<FieldCard>().parentCell.cellFull = false;
                Destroy(gameObject);
            }
            BattlePhaseManager.phase++;
        }
    }
}
