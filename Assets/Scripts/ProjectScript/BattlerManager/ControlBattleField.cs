using ProjectScript.Enums;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlBattleField : MonoBehaviour
{
    public int AttributeBonus = 200; // Bônus de atributo para o atacante

    public TextMeshProUGUI topLevelTextRed;
    public TextMeshProUGUI topMemoryTextRed;
    public TextMeshProUGUI topLevelTextBlue;
    public TextMeshProUGUI topMemoryTextBlue;

    private PlayerSetup setupBlue;
    private PlayerSetup setupRed;

    // Designações explícitas de enfrentamento entre grids
    private Dictionary<int, int> battlePairings = new Dictionary<int, int>();

    private void Awake()
    {
        SetupBattlePairings();
    }
    private void Start()
    {
        setupBlue = GameSetupStart.playerBlue;
        setupRed = GameSetupStart.playerRed;
    }
    private void Update()
    {
        if(setupBlue == null || setupRed == null)
        {
            if (GameSetupStart.playerBlue == null)
            {
                Debug.Log("erro no gamesetupstart");
            }
            setupBlue = GameSetupStart.playerBlue;
            setupRed = GameSetupStart.playerRed;
            Debug.Log("erro");
        }
        topLevelTextBlue.text = $"Level: {setupBlue.maxLevelPartner}";
        topMemoryTextBlue.text = $"Memory: {setupBlue.currentMemory} / {setupBlue.maxMemory}";

        topLevelTextRed.text = $"Level: {setupRed.maxLevelPartner}";
        topMemoryTextRed.text = $"Memory: {setupRed.currentMemory} / {setupRed.maxMemory}";

        UpdateCurrentMemoryFromField();
    }
    
    // memory settings
    internal int GetTopLevel(PlayerSide playerSide)
    {
        if (setupBlue.setPlayer == playerSide)
            return setupBlue.maxLevelPartner;
        else
            return setupRed.maxLevelPartner;
    }
    internal int GetTopMemory(PlayerSide playerSide)
    {
        if (setupBlue.setPlayer == playerSide)
            return setupBlue.maxMemory;
        else
            return setupRed.maxMemory;
    }
    internal int GetTotalLevel(PlayerSide playerSide)
    {
        if (setupBlue.setPlayer == playerSide)
            return setupBlue.currentMemory;
        else
            return setupRed.currentMemory;
    }
    private void UpdateCurrentMemoryFromField()
    {
        setupBlue.currentMemory = 0;
        setupRed.currentMemory = 0;

        var digimons = DigimonDisplay.AllDigimons;

        foreach (var digimon in digimons)
        {
            if (digimon == null) continue;

            FieldCard fieldCard = digimon.GetComponent<FieldCard>();
            if (fieldCard == null || fieldCard.parentCell == null) continue;

            PlayerSide owner = fieldCard.GetFieldOwner();

            switch (owner)
            {
                case PlayerSide.PlayerBlue:
                    setupBlue.currentMemory += digimon.level;
                    topMemoryTextBlue.color = setupBlue.currentMemory == setupBlue.maxMemory ? Color.green : Color.white;
                    break;

                case PlayerSide.PlayerRed:
                    setupRed.currentMemory += digimon.level;
                    topMemoryTextRed.color = setupRed.currentMemory == setupRed.maxMemory ? Color.green : Color.white;
                    break;
            }
        }
    }

    // combat settings
    private void SetupBattlePairings()
    {
        // Designação manual dos grids que se enfrentam
        battlePairings[1] = 5;
        battlePairings[2] = 6;
        battlePairings[3] = 7;
        // Bidirecional
        battlePairings[5] = 1;
        battlePairings[6] = 2;
        battlePairings[7] = 3;
    }
    public void GetOpponentAtFront(GridCell currentCell, PlayerSide playerSide, GameObject attackerObj)
    {
        PlayerSide opponentSide = playerSide == PlayerSide.PlayerBlue ? PlayerSide.PlayerRed : PlayerSide.PlayerBlue;
        PlayerSetup opponentPlayer = GameSetupStart.GetPlayerSetup(opponentSide);
        TriggerCardManager.TriggerAttacking();
        // Se o atacante estiver na layer 11, forçar ataque direto
        if (attackerObj != null && attackerObj.layer == 11)
        {
            Debug.Log("[GetOpponentAtFront] Atacante está na layer 11, forçando ataque direto.");
            opponentPlayer.securityPile.DestroySecurity();
            return;
        }

        if (currentCell == null)
        {
            Debug.LogError("[GetOpponentAtFront] currentCell está nulo.");
            return;
        }

        if (!battlePairings.TryGetValue(currentCell.gridIndex, out int opponentGridID))
        {
            Debug.LogError($"[GetOpponentAtFront] Nenhum oponente designado para o gridIndex {currentCell.gridIndex}");
            return;
        }

        // Busca a célula adversária baseada no gridIndex do oponente
        GridCell opponentCell = null;

        // Percorre todos os digimons para obter a célula do oponente
        foreach (var digimon in DigimonDisplay.AllDigimons)
        {
            if (digimon == null) continue;

            FieldCard fieldCard = digimon.GetComponent<FieldCard>();
            if (fieldCard == null || fieldCard.parentCell == null) continue;

            if (fieldCard.parentCell.gridIndex == opponentGridID)
            {
                opponentCell = fieldCard.parentCell;
                break;
            }
        }
        Debug.Log($"[GetOpponentAtFront] Verificando célula adversária. opponentCell: {opponentCell}, cellFull: {opponentCell?.cellFull}");

        // Se não encontrou nenhuma carta na célula adversária, então a célula está vazia (ataque direto)
        if (opponentCell == null || !opponentCell.cellFull)
        {
            Debug.Log($"[GetOpponentAtFront] Ataque direto do lado {playerSide} porque a célula adversária {opponentGridID} está vazia.");
            opponentPlayer.securityPile.DestroySecurity();
            return;
        }
        // Se a célula adversária está ocupada, procura o oponente para combate
        Debug.Log($"[GetOpponentAtFront] {playerSide} está na célula {currentCell.gridIndex}, procurando oponente na célula {opponentGridID}");

        foreach (var digimon in DigimonDisplay.AllDigimons)
        {
            if (digimon == null) continue;

            FieldCard fieldCard = digimon.GetComponent<FieldCard>();
            if (fieldCard == null || fieldCard.parentCell == null) continue;

            if (fieldCard.parentCell.gridIndex == opponentGridID)
            {
                Debug.Log($"[GetOpponentAtFront] Oponente encontrado: '{digimon.cardName}' na célula {opponentGridID}");
                ResolveCombat(attackerObj, digimon.gameObject);
                return;
            }
        }
        Debug.LogWarning($"[GetOpponentAtFront] Nenhum oponente encontrado na frente (grid {opponentGridID}).");     
    }
    private bool HasAttributeAdvantage(DigimonAttribute attackerAttr, DigimonAttribute defenderAttr)
    {
        // Unknown sempre tem vantagem (exceto contra Free)
        if (attackerAttr == DigimonAttribute.Unknown && defenderAttr != DigimonAttribute.Free)
            return true;

        // Free tem vantagem apenas contra Unknown
        if (attackerAttr == DigimonAttribute.Free && defenderAttr == DigimonAttribute.Unknown)
            return true;

        // Regras principais (atacante vence)
        if (attackerAttr == DigimonAttribute.Virus && defenderAttr == DigimonAttribute.Data) return true;
        if (attackerAttr == DigimonAttribute.Data && defenderAttr == DigimonAttribute.Vaccine) return true;
        if (attackerAttr == DigimonAttribute.Vaccine && defenderAttr == DigimonAttribute.Virus) return true;

        return false;
    }
    public void ResolveCombat(GameObject attackerObj, GameObject defenderObj)
    {
        DigimonDisplay digiAttacker = attackerObj.GetComponent<DigimonDisplay>();
        DigimonDisplay digiDefender = defenderObj.GetComponent<DigimonDisplay>();
        int attackerPower = digiAttacker.power;
        int defenderPower = digiDefender.power;

        if(HasAttributeAdvantage(digiAttacker.attribute, digiDefender.attribute))
        {
            attackerPower += AttributeBonus;
        }

        if(attackerPower <= defenderPower)
        {
            attackerObj.GetComponent<FieldCard>().DestroyFieldCard();
        }
        else
        {
            defenderObj.GetComponent<FieldCard>().DestroyFieldCard();
        }
    }

    // Deck list
    public void OnClickPartnerPileZone(PlayerSetup playerSide)
    {
        Debug.Log($"Clicou na PartnerPile {playerSide}");

        HideAllPartnerPiles();
        playerSide.partnerPile.ShowPartnerPile();
        playerSide.hand.HideHand();
        playerSide.partnerPile.UpdatePartnerPileCostColors();
    }

    public void ShowHandForBoth()
    {
        setupBlue.GetComponent<PlayerSetup>().hand.handTransform.gameObject.SetActive(true);
        setupRed.GetComponent<PlayerSetup>().hand.handTransform.gameObject.SetActive(true);
    }

    public void HideAllPartnerPiles()
    {
        if (setupBlue == null || setupRed == null)
        {
            Debug.LogError("PlayerSetup não está atribuído corretamente no ControlBattleField.");
            return;
        }
        setupBlue.GetComponent<PlayerSetup>().partnerPile.partnerPileTransform.gameObject.SetActive(false);
        setupRed.GetComponent<PlayerSetup>().partnerPile.partnerPileTransform.gameObject.SetActive(false);
        setupBlue.GetComponent<PlayerSetup>().evoPile.rectEvoPile.gameObject.SetActive(false);
        setupRed.GetComponent<PlayerSetup>().evoPile.rectEvoPile.gameObject.SetActive(false);
    }
}
