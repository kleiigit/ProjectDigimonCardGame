using System;
using System.Collections.Generic;
using UnityEngine;

namespace SinuousProductions
{
    [Serializable]
    public class SavedCardEntry
    {
        public string cardID;
        public CardStatus status;
        public int quantity;

        public SavedCardEntry(string cardID, CardStatus status, int quantity)
        {
            this.cardID = cardID;
            this.status = status;
            this.quantity = quantity;
        }
    }

    [Serializable]
    public class SaveData
    {
        public string playerName;
        public List<string> inventory = new();
        public float[] position = new float[3];
        public List<SavedCardEntry> discoveredCards = new();

        public List<DeckData> savedDecks = new(); // <-- Lista de baralhos salvos

        public int GetDiscoveredCardCount()
        {
            if (discoveredCards == null)
            {
                Debug.LogWarning("[SaveData] discoveredCards está null.");
                return 0;
            }

            int count = 0;
            foreach (var entry in discoveredCards)
            {
                if (entry.quantity > 0)
                {
                    count++;
                    Debug.Log($"[SaveData] Verificando carta: {entry.cardID} | Status: {entry.status} | Quantidade: {entry.quantity}");
                }
            }

            Debug.Log($"[SaveData] Total de cartas possuídas com quantidade > 0: {count}");
            return count;
        }
    }


}
