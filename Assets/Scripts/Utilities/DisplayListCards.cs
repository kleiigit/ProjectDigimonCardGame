using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SinuousProductions.Card;

namespace SinuousProductions
{
    public class DisplayListCards : MonoBehaviour
    {
        public static DisplayListCards Instance;

        [Header("Referências")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private RectTransform cardGridContent;
        [SerializeField] private GameObject displayListObj;
        [SerializeField] private TMPro.TextMeshProUGUI panelDescriptionText;

        private List<GameObject> instantiatedCards = new();
        private Dictionary<string, int> deckCardWeights = new Dictionary<string, int>();

        private bool isPanelOpen = false;
        public bool collectionCards = true;



        private void Update()
        {
            if (!isPanelOpen) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    if (IsPointerOverDisplayObj()) return;
                }
            }
        }

        public void SetCardGridLayout(Vector2? size = null, Vector2? position = null)
        {
            if (cardGridContent == null) return;

            if (size.HasValue)
                cardGridContent.sizeDelta = size.Value;

            if (position.HasValue)
                cardGridContent.anchoredPosition = position.Value;
        }

        private bool IsPointerOverDisplayObj()
        {
            if (displayListObj == null) return false;

            RectTransform rectTransform = displayListObj.GetComponent<RectTransform>();
            if (rectTransform == null) return false;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );

            return rectTransform.rect.Contains(localPoint);
        }

        public void ShowCardList(List<Card> cardList, string panelDescription, bool useCollectionMode)
        {
            ClearPrevious();

            if (cardList == null || cardList.Count == 0)
            {
                Debug.LogWarning("[DisplayListCards] Lista de cartas vazia ou nula.");
                return;
            }

            displayListObj.SetActive(true);
            isPanelOpen = true;

            if (panelDescriptionText != null)
                panelDescriptionText.text = panelDescription;

            foreach (Card card in cardList)
            {
                if (card == null) continue;

                if (!useCollectionMode)
                {
                    GameObject cardObj = Instantiate(cardPrefab, cardGridContent, false);
                    cardObj.name = card.cardID;
                    cardObj.layer = 14;

                    var display = cardObj.GetComponent<CardDisplay>();
                    if (display != null)
                    {
                        display.cardData = card;
                        display.UpdateCardDisplay();
                    }

                    CardsCollectionManager.Instance.ApplyVisualStatus(cardObj, card.cardID);
                    instantiatedCards.Add(cardObj);
                }
                else
                {
                    int quantity = CardsCollectionManager.Instance.GetCardQuantity(card.cardID);
                    CardStatus status = CardsCollectionManager.Instance.GetCardStatus(card.cardID);

                    if (status == CardStatus.Seen)
                    {
                        GameObject cardObj = Instantiate(cardPrefab, cardGridContent, false);
                        cardObj.name = card.cardID;
                        cardObj.layer = 14;

                        var display = cardObj.GetComponent<CardDisplay>();
                        if (display != null)
                        {
                            display.cardData = card;
                            display.UpdateCardDisplay();
                        }

                        CardsCollectionManager.Instance.ApplyVisualStatus(cardObj, card.cardID);
                        instantiatedCards.Add(cardObj);
                    }
                    else if (status == CardStatus.Owned && quantity > 0)
                    {
                        if (collectionCards)
                        {
                            GameObject cardObj = Instantiate(cardPrefab, cardGridContent, false);
                            cardObj.name = card.cardID;
                            cardObj.layer = 14;

                            var display = cardObj.GetComponent<CardDisplay>();
                            if (display != null)
                            {
                                display.cardData = card;
                                display.UpdateCardDisplay();
                            }

                            Transform cardCanvas = cardObj.transform.Find("CardCanvas");
                            if (cardCanvas != null)
                            {
                                GameObject counterObj = new GameObject("CardCounter");
                                counterObj.transform.SetParent(cardCanvas.transform, false);

                                Image bg = counterObj.AddComponent<Image>();
                                bg.color = new Color(0, 0, 0, 0.6f);
                                bg.raycastTarget = false;

                                RectTransform rt = counterObj.GetComponent<RectTransform>();
                                rt.anchorMin = new Vector2(1, 0);
                                rt.anchorMax = new Vector2(1, 0);
                                rt.pivot = new Vector2(1, 0);
                                rt.anchoredPosition = new Vector2(0, 0);
                                rt.sizeDelta = new Vector2(1.5f, 1.5f);

                                GameObject textObj = new GameObject("CounterText");
                                textObj.transform.SetParent(counterObj.transform, false);

                                TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
                                text.text = $"x{quantity}";
                                text.fontSize = 1;
                                text.alignment = TMPro.TextAlignmentOptions.Center;
                                text.color = Color.white;
                                text.raycastTarget = false;
                                
                                RectTransform textRT = text.GetComponent<RectTransform>();
                                textRT.anchorMin = Vector2.zero;
                                textRT.anchorMax = Vector2.one;
                                textRT.offsetMin = Vector2.zero;
                                textRT.offsetMax = Vector2.zero;
                            }

                            CardsCollectionManager.Instance.ApplyVisualStatus(cardObj, card.cardID);
                            instantiatedCards.Add(cardObj);
                        }
                        else
                        {
                            for (int i = 0; i < quantity; i++)
                            {
                                GameObject cardObj = Instantiate(cardPrefab, cardGridContent, false);
                                cardObj.name = card.cardID;
                                cardObj.layer = 14;

                                var display = cardObj.GetComponent<CardDisplay>();
                                if (display != null)
                                {
                                    display.cardData = card;
                                    display.UpdateCardDisplay();
                                }

                                CardsCollectionManager.Instance.ApplyVisualStatus(cardObj, card.cardID);
                                instantiatedCards.Add(cardObj);
                            }
                        }
                    }
                }
            }
        }

        public void AddCardToDisplay(Card card)
        {
            if (card == null) return;

            GameObject cardObj = Instantiate(cardPrefab, cardGridContent, false);
            cardObj.name = card.cardID;
            cardObj.layer = 14;

            var display = cardObj.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.cardData = card;
                display.UpdateCardDisplay();
            }

            CardsCollectionManager.Instance.ApplyVisualStatus(cardObj, card.cardID);
            instantiatedCards.Add(cardObj);
        }

        public void ClearPrevious()
        {
            foreach (var card in instantiatedCards)
            {
                if (card != null)
                    Destroy(card);
            }
            instantiatedCards.Clear();
        }

        public void Hide()
        {
            ClearPrevious();
            displayListObj.SetActive(false);
            isPanelOpen = false;
        }

        public void SetCardGridContent(RectTransform newContent)
        {
            cardGridContent = newContent;
        }
    }
}
