using ProjectScript.Enums;
using ProjectScript.Interfaces;
using SinuousProductions;
using System.Collections.Generic;
using UnityEngine;

public class DrawPileManager : MonoBehaviour, IPile
{
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
        if (rechargeWaiting && setup.listDiscardObj.Count > 0)
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
                    Debug.Log($"[CardDrawController] Adicionando carta à mão do jogador Red: {nextCard.cardName}");
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

        }
    }
    public void DrawCardToDataPile(int amount)
    {
        DrawCard(amount, FieldPlace.DataPile);
    }
    public void RefillDeckFromDiscard()
    {
        if (setup.listDiscardObj.Count == 0)
        {
            rechargeWaiting = true;
            Debug.LogWarning($"[DrawPileManager] Sem cartas na lixeira para recarregar {setup.setPlayer}");
            return;
        }

        // Obtem lista de GameObjects do descarte
        List<GameObject> discardedObjects = discardManager.PullAllFromDiscard(setup.setPlayer);

        // Converte para lista de Cards pegando o componente Card de cada GameObject
        List<Card> cards = new List<Card>();
        foreach (GameObject go in discardedObjects)
        {
            Card card = go.GetComponent<Card>();
            if (card != null)
                cards.Add(card);
            else
                Debug.LogWarning($"GameObject {go.name} não possui componente Card");
        }

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

    public void RemoveCard(GameObject cardObject)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateVisuals()
    {
        throw new System.NotImplementedException();
    }
}
