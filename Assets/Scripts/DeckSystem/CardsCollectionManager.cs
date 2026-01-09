using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SinuousProductions
{
    public enum CardStatus
    {
        Undiscovered,
        Seen,
        Owned
    }

    [System.Serializable]
    public class CardData
    {
        public Card card;
        public CardStatus status;
        public int quantity;

        public CardData(Card card)
        {
            this.card = card;
            this.status = CardStatus.Undiscovered;
            this.quantity = 0;
        }
    }

    public class CardsCollectionManager : MonoBehaviour
    {
        public static CardsCollectionManager Instance;
        public Material grayscaleMaterial;
        private LayerMask layerMask = 14;

        [Header("Visuais")]

        private Dictionary<string, CardData> cardCollection = new();

        // Dicionário para armazenar as cores originais dos componentes Image de cada GameObject de carta
        private Dictionary<GameObject, Color[]> originalColorsDict = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Garante que este objeto seja root para evitar erro do DontDestroyOnLoad
                transform.SetParent(null);
                

                InitializeCardCollection();
            }
            
        }

        private void InitializeCardCollection()
        {
            Card[] allCards = Resources.LoadAll<Card>("Cards");

            foreach (Card card in allCards)
            {
                if (!cardCollection.ContainsKey(card.cardID))
                {
                    cardCollection.Add(card.cardID, new CardData(card));
                }
            }
        }

        public void MarkAsSeen(string cardId)
        {
            if (cardCollection.TryGetValue(cardId, out CardData data))
            {
                if (data.status == CardStatus.Undiscovered)
                    data.status = CardStatus.Seen;
            }
        }

        public void AddCard(string cardId, int amount = 1)
        {
            if (cardCollection.TryGetValue(cardId, out CardData data))
            {
                data.quantity += amount;

                if (data.quantity > 0)
                    data.status = CardStatus.Owned;
                else if (data.status != CardStatus.Undiscovered)
                    data.status = CardStatus.Seen;
            }
        }

        public CardStatus GetCardStatus(string cardId)
        {
            return cardCollection.TryGetValue(cardId, out CardData data)
                ? data.status
                : CardStatus.Undiscovered;
        }

        public int GetCardQuantity(string cardId)
        {
            return cardCollection.TryGetValue(cardId, out CardData data)
                ? data.quantity
                : 0;
        }

        public void ApplyVisualStatus(GameObject cardObject, string cardId)
        {
            if (!cardCollection.ContainsKey(cardId)) return;

            CardStatus status = cardCollection[cardId].status;

            // Busca pelo filho "MainBack" para overlay
            Transform overlayTransform = cardObject.transform.Find("MainBack");
            GameObject overlay = overlayTransform != null ? overlayTransform.gameObject : null;

            switch (status)
            {
                case CardStatus.Undiscovered:
                    if (overlay != null)
                        overlay.SetActive(true);
                    break;

                case CardStatus.Seen:
                    if (overlay != null)
                        overlay.SetActive(false);

                    SetCardDark(cardObject, true); // escurece a carta
                    break;

                case CardStatus.Owned:
                    if (overlay != null)
                        overlay.SetActive(false);

                    SetCardDark(cardObject, false); // cores normais
                    break;
            }
        }

        /// <summary>
        /// Aplica escurecimento nas imagens do GameObject da carta.
        /// </summary>
        /// <param name="cardObject">GameObject da carta.</param>
        /// <param name="darken">Se true, escurece a carta; se false, restaura cores originais.</param>
        public void SetCardDark(GameObject cardObject, bool grayScale)
        {
            var allImages = cardObject.GetComponentsInChildren<Image>(includeInactive: true);

            if (!originalColorsDict.ContainsKey(cardObject))
            {
                Color[] originalColors = new Color[allImages.Length];
                for (int i = 0; i < allImages.Length; i++)
                {
                    originalColors[i] = allImages[i].color;
                }
                originalColorsDict[cardObject] = originalColors;
            }

            var colors = originalColorsDict[cardObject];

            for (int i = 0; i < allImages.Length; i++)
            {
                allImages[i].material = grayScale ? grayscaleMaterial : null;
                allImages[i].color = colors[i];
            }
        }



        /// <summary>
        /// Retorna todas as cartas do jogo, independentemente do status.
        /// </summary>
        public List<Card> GetAllCardsInGame()
        {
            List<Card> allCards = new();
            foreach (var entry in cardCollection)
            {
                if (entry.Value != null && entry.Value.card != null)
                    allCards.Add(entry.Value.card);
            }
            return allCards;
        }

        /// <summary>
        /// Exibe todas as cartas do jogo usando o painel DisplayListCards.
        /// </summary>
        public void ShowAllCardsInGame(DisplayListCards display)
        {
            List<Card> allCards = GetAllCardsInGame();

            if (display != null)
            {
                Debug.Log("Chamando DisplayListCards.ShowCardList com " + allCards.Count + " cartas.");
                display.ShowCardList(allCards, layerMask, "Cartas da Coleção", true);
            }
            else
            {
                Debug.LogWarning("[CardsCollectionManager] DisplayListCards não informado para exibir a coleção.");
            }
        }

        /// <summary>
        /// Retorna uma lista serializável contendo apenas cartas já vistas ou obtidas.
        /// </summary>
        public List<SavedCardEntry> GetDiscoveredCardEntries()
        {
            List<SavedCardEntry> saved = new();

            foreach (var entry in cardCollection)
            {
                if (entry.Value.status == CardStatus.Seen || entry.Value.status == CardStatus.Owned)
                {
                    // Salva mesmo se quantity for 0 para cartas Seen
                    saved.Add(new SavedCardEntry(
                        entry.Key,
                        entry.Value.status,
                        entry.Value.status == CardStatus.Owned ? entry.Value.quantity : 0
                    ));
                    Debug.Log($"Salvando carta {entry.Key} com status {entry.Value.status} e quantidade {entry.Value.quantity}");
                }
            }

            return saved;
        }

        /// <summary>
        /// Aplica os dados salvos à coleção atual.
        /// </summary>
        public void ApplyDiscoveredCardEntries(List<SavedCardEntry> savedEntries)
        {
            InitializeCardCollection();

            foreach (var saved in savedEntries)
            {
                if (cardCollection.TryGetValue(saved.cardID, out CardData data))
                {
                    data.status = saved.status;
                    data.quantity = saved.status == CardStatus.Owned ? saved.quantity : 0;
                    //Debug.Log($"Carregando carta {saved.cardID} com status {saved.status} e quantidade {saved.quantity}");
                }
            }
        }
        #region(Test Methods)

        /// <summary>
        /// Marca todas as cartas como Seen (vistos) e quantidade 0.
        /// </summary>
        [ContextMenu("Test - Marcar Todas Como Seen")]
        public void MarkAllAsSeen()
        {
            foreach (var entry in cardCollection.Values)
            {
                entry.status = CardStatus.Seen;
                entry.quantity = 0;
            }
            Debug.Log("[Test] Todas as cartas marcadas como Seen.");
        }

        /// <summary>
        /// Marca todas as cartas como Owned (obtidas) com quantidade especificada.
        /// </summary>
        /// <param name="quantity">Quantidade para todas as cartas (padrão 1).</param>
        public void MarkAllAsOwned(int quantity = 1)
        {
            foreach (var entry in cardCollection.Values)
            {
                entry.status = CardStatus.Owned;
                entry.quantity = Mathf.Max(0, quantity);
            }
            Debug.Log($"[Test] Todas as cartas marcadas como Owned com quantidade {quantity}.");
        }

        /// <summary>
        /// Reseta todas as cartas para Undiscovered e quantidade 0.
        /// </summary>
        [ContextMenu("Test - Remover Todas As Cartas")]
        public void ClearAllCards()
        {
            foreach (var entry in cardCollection.Values)
            {
                entry.status = CardStatus.Undiscovered;
                entry.quantity = 0;
            }
            Debug.Log("[Test] Todas as cartas removidas (Undiscovered, quantidade 0).");
        }
        #endregion
    }
}

