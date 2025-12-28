using System.Linq;
using TMPro;
using UnityEngine;
using ProjectScript.Enums;
using System.Collections.Generic;

public class BattlePhaseManager : MonoBehaviour
{
    public static Phase phase;
    public static int roundCount = 0;
    public static PlayerSide currentPlayer = PlayerSide.PlayerBlue;

    // Instancias
    private ControlBattleField control;
    private PartnerPileManager partnerPileManager;
    private MenuCardManager menuCardManager;
    private PlayerSetup playerSetupBlue;
    private PlayerSetup playerSetupRed;

    // Referencias UI
    public GameObject battlePhasePanel;
    public TMP_Text battlePhaseText;
    public TMP_Text counterText;
    public TMP_Text currentPlayerText;
    public TMP_Text buttonPhaseNext;
    public GameObject buttonNextPhaseObj;


    private void Start()
    {
        // inicializações
        partnerPileManager = FindFirstObjectByType<PartnerPileManager>();
        control = FindFirstObjectByType<ControlBattleField>();

        playerSetupBlue = GameSetupStart.playerBlue;
        playerSetupRed = GameSetupStart.playerRed;

        Debug.Log("Turno inicial: " + currentPlayer);
    }

    private void Update()
    {
        // Round Counter Update
        counterText.text = "Rounds: " + roundCount;
        if (roundCount >= 1)
        {
            buttonNextPhaseObj.gameObject.SetActive(true);
            UpdatePhase(); 
        }


        // Display Current Player in Text
        if (currentPlayerText != null)
        {
            currentPlayerText.text = "Current Player: " + currentPlayer.ToString();
        }

        currentPlayerText.color = currentPlayer == PlayerSide.PlayerBlue ? Color.blue : Color.red;

        // Check for draw pile refill to end turn
        if (control.GetPlayerSetup(currentPlayer).refill == true)
        {
            phase = Phase.EndPhase;
        }
    }

    private void UpdatePhase()
    {
        switch (phase)
        {
            case Phase.UpPhase:
                battlePhasePanel.SetActive(true);
                
                buttonPhaseNext.text = "Pular";
                battlePhaseText.text = "Fase de Virar".ToUpper();
                UpdateCardButtonControl(null);
                NextPhase();
                break;

            case Phase.DrawPhase:
                battlePhasePanel.SetActive(true);
                battlePhaseText.text = "Fase de Compra".ToUpper();

                int count = roundCount == 1 ? 1 : 2;
                if(currentPlayer == playerSetupBlue.setPlayer)
                {
                    playerSetupBlue.drawPile.DrawCard(count, FieldPlace.Hand);
                }
                else
                {
                    playerSetupRed.drawPile.DrawCard(count, FieldPlace.Hand);
                }

                UpdateCardButtonControl(null);
                NextPhase();
                break;

            case Phase.CostPhase:
                battlePhaseText.text = "Fase de Data".ToUpper();
                UpdateCardButtonControl(currentPlayer);
                break;

            case Phase.EvolutionPhase:
                battlePhaseText.text = "Fase de Evolução".ToUpper();
                UpdateCardButtonControl(currentPlayer);
                if(currentPlayer == playerSetupBlue.setPlayer)
                {
                    playerSetupBlue.partnerPile.ShowPartnerPile();
                }
                else
                {
                    playerSetupRed.partnerPile.ShowPartnerPile();
                }
                    break;

            case Phase.MainPhase:
                if (roundCount == 1) buttonPhaseNext.text = "Finalizar o turno";

                battlePhaseText.text = "Fase de Principal".ToUpper();
                UpdateCardButtonControl(currentPlayer);
                break;

            case Phase.SkillPhase:
                battlePhaseText.text = "Fase de Preparação".ToUpper();
                UpdateCardButtonControl(currentPlayer);
                break;

            case Phase.AttackPhase:
                battlePhaseText.text = "Fase de Ataque".ToUpper();
                UpdateCardButtonControl(currentPlayer);
                break;
            case Phase.EndPhase:
                battlePhaseText.text = "Fase de Final".ToUpper();

                if (currentPlayer == playerSetupBlue.setPlayer)
                {
                    playerSetupBlue.refill = false;
                }
                else
                {
                    playerSetupBlue.refill = false;
                }
                UpdateCardButtonControl(null);
                /*if (hand.cardsInHands[currentPlayer].Count > setupStart.maxEndTurnSize)
                {
                    Debug.LogWarning("cards acima do limite");
                }
                else
                {
                    NextPhase();
                    return; 
                }*/
                break;
        }
    }


    public void NextPhase()
    {
        // Se for a fase final, incrementa o contador de turnos e reseta para a fase de virada
        if (phase == Phase.EndPhase)
        {
            roundCount++;
            phase = Phase.UpPhase;

            // Alterna jogador
            currentPlayer = (currentPlayer == PlayerSide.PlayerBlue) ? PlayerSide.PlayerRed : PlayerSide.PlayerBlue;

            Debug.Log("Turno de " + currentPlayer);
            return;
        }

        // Restrições de fase
        if (roundCount == 1 && phase == Phase.MainPhase)
        {
            Debug.Log("Não pode ir para fase de ataque no primeiro turno da fase principal.");
            phase = Phase.EndPhase;
            return;
        }

        // Incrementa o contador de turnos na primeira passagem
        if (roundCount == 0)
        {
            roundCount++;
        }

        if (phase == Phase.EvolutionPhase) partnerPileManager.HidePartnerPile();

        phase++;
    }

    public void UpdateCardButtonControl(PlayerSide? currentPlayer)
    {
        // nao esta funcionando
    }

}




