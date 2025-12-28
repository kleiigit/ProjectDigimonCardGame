using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;  // para Debug.Log no Unity

namespace SinuousProductions
{
    [Serializable]
    public class DeckCardEntry
    {
        public string cardID;
        public int quantity;

        public DeckCardEntry(string cardID, int quantity)
        {
            this.cardID = cardID;
            this.quantity = quantity;
        }
    }

    [Serializable]
    public class DeckData
    {
        public string deckName;
        public List<DeckCardEntry> mainDeck = new();
        public List<DeckCardEntry> partnerDeck = new();
    }

    public static class DeckConverter
    {
        public static DeckData ToDeckData(string name, List<Card> mainDeck, List<Card> partnerDeck)
        {
            var mainGrouped = mainDeck
                .GroupBy(c => c.cardID)
                .Select(g => new DeckCardEntry(g.Key, g.Count()))
                .ToList();

            var partnerGrouped = partnerDeck
                .GroupBy(c => c.cardID)
                .Select(g => new DeckCardEntry(g.Key, g.Count()))
                .ToList();

            return new DeckData
            {
                deckName = name,
                mainDeck = mainGrouped,
                partnerDeck = partnerGrouped
            };
        }

        public static void FromDeckData(
            DeckData data,
            out List<Card> mainDeck,
            out List<Card> partnerDeck,
            List<Card> cardDatabase)
        {
            mainDeck = new List<Card>();
            foreach (var entry in data.mainDeck)
            {
                var card = cardDatabase.FirstOrDefault(c => c.cardID == entry.cardID);
                if (card != null)
                {
                    for (int i = 0; i < entry.quantity; i++)
                    {
                        mainDeck.Add(card);
                    }
                }

            }

            partnerDeck = new List<Card>();
            foreach (var entry in data.partnerDeck)
            {
                var card = cardDatabase.FirstOrDefault(c => c.cardID == entry.cardID);
                if (card != null)
                {
                    for (int i = 0; i < entry.quantity; i++)
                    {
                        partnerDeck.Add(card);
                    }
                }

            }
        }
    }
}
