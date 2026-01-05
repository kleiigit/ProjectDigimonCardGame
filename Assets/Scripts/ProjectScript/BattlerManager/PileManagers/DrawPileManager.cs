using ProjectScript.Enums;
using ProjectScript.Interfaces;
using SinuousProductions;
using System.Collections.Generic;
using UnityEngine;

public class DrawPileManager : MonoBehaviour, IPile
{
    [SerializeField] Transform deckTransform;
    private bool rechargeWaiting = false;

    private PlayerSetup setup;
    private DiscardManager discardManager;

    void Start()
    {
        discardManager = FindFirstObjectByType<DiscardManager>();
        setup = gameObject.GetComponent<PlayerSetup>();
    }

    void Update()
    {
        if (rechargeWaiting && setup.listDiscardCards.Count > 0)
        {
            RefillDeckFromDiscard();
            rechargeWaiting = false;
        }
    }
    public void DrawCard(int amount, FieldPlace destination)
    {
        for (int i = 0; i < amount; i++)
        {
            if (setup.playerDeck.Count == 0)
            {
                Debug.LogWarning($"[CardDrawController] drawPiles[{setup.setPlayer}] vazio ou inexistente. Tamanho atual: {setup.playerDeck.Count}");
                RefillDeckFromDiscard();

                if (setup.playerDeck.Count == 0)
                {
                    Debug.LogError($"[CardDrawController] Sem cartas para comprar para o lado {setup.setPlayer} após tentativa de recarregar.");
                    break;
                }
            }

            Card nextCard = setup.playerDeck[0];
            setup.playerDeck.RemoveAt(0);

            switch (destination)
            {
                case FieldPlace.Hand:
                    //Debug.Log($"[CardDrawController] Adicionando carta à mão do jogador Red: {nextCard.cardName}");
                    setup.hand.AddCard(nextCard);
                    break;

                case FieldPlace.DataPile:
                    setup.dataPile.AddCard(nextCard);
                    break;

                case FieldPlace.TrashPile:
                    setup.discard.AddCard(nextCard);
                    break;
                case FieldPlace.SecurityPile:
                    setup.securityPile.AddCard(nextCard);
                    break;

                default:
                    Debug.LogWarning($"Destino inválido: {destination}. Carta descartada.");
                    break;
            }
            TriggerCardManager.TriggerDrawing();
        }
    }
    public void DrawCardToDataPile(int amount)
    {
        DrawCard(amount, FieldPlace.DataPile);
    }
    public void RefillDeckFromDiscard()
    {
        if (setup.listDiscardCards.Count == 0)
        {
            rechargeWaiting = true;
            Debug.LogWarning($"[DrawPileManager] Sem cartas na lixeira para recarregar {setup.setPlayer}");
            return;
        }
        List<Card> cards = new List<Card>(discardManager.PullAllFromDiscard(setup.setPlayer));

        setup.playerDeck = cards;

        Utility.Shuffle(setup.playerDeck);
        setup.refill = true;
    }

    public static void BattleSetupHand(int amountPerPlayer)
    {
        GameSetupStart.playerBlue.drawPile.DrawCard(amountPerPlayer, FieldPlace.Hand);
        GameSetupStart.playerRed.drawPile.DrawCard(amountPerPlayer, FieldPlace.Hand);
    }

    public void AddCard(Card cardData)
    {
        throw new System.NotImplementedException();
    }
    public void RemoveCard(GameObject cardObject) { }

    public void UpdateVisuals()
    {
        
    }
}
