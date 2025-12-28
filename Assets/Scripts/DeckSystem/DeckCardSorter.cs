using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ProjectScript.Enums;

namespace SinuousProductions
{
    public enum SortMode
    {
        Default,
        LevelAsc, // Asc e Desc nao podem ser usados juntos / Corrigir isso
        LevelDesc,
        TypeMode1, // Digimon, Partner, Program, Skill
        TypeMode2, // Partner, Skill, Digimon, Program
        TypeMode3,
        NameAZ,
        NameZA,
        CostAsc,
        CostDesc,
        Color,
        StageAsc,
        StageDesc,
        QuantityAsc,
        QuantityDesc,
        PowerAsc,
        PowerDesc,
        CardIDAsc,
        CardIDDesc,
        Field,
        Attribute,
        TypeDigimon,
        ViewDeckOrder,
    }

    public static class DeckCardSorter
    {
        private static readonly Dictionary<CardType, int> CardTypeOrder = new()
        {
            { CardType.Digimon, 0 },
            { CardType.Program, 1 },
            { CardType.Partner, 2 },
            { CardType.Skill, 3 },
            { CardType.Card, 4 },
        };

        private static SortMode currentSortMode = SortMode.Default;
        private static SortMode? previousSortMode = null;

        private static GameObject optionsPanel;
        private static Button floatingButton;
        private static Dictionary<SortMode, Button> sortButtons;
        private static List<Card> currentCardList;
        private static System.Action<List<Card>> onSortedCallback;
        private static Dictionary<string, int> cardQuantities;

        public static void InitializeSorterUI(
            Button mainButton,
            GameObject sortOptionsPanel,
            Dictionary<SortMode, Button> buttons,
            List<Card> cardListReference,
            Dictionary<string, int> quantityReference,
            System.Action<List<Card>> callbackAfterSort = null)
        {
            floatingButton = mainButton;
            optionsPanel = sortOptionsPanel;
            sortButtons = buttons;
            currentCardList = cardListReference;
            cardQuantities = quantityReference;
            onSortedCallback = callbackAfterSort;

            if (floatingButton != null)
                floatingButton.onClick.AddListener(() =>
                {
                    if (optionsPanel != null)
                        optionsPanel.SetActive(!optionsPanel.activeSelf);
                });

            foreach (var pair in sortButtons)
            {
                SortMode mode = pair.Key;
                pair.Value.onClick.AddListener(() => ApplySort(mode));
            }

            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }

        public static void ApplySort(SortMode mode)
        {
            ToggleSortMode(mode);

            if (currentCardList == null)
                return;

            SortCards(currentCardList);

            onSortedCallback?.Invoke(currentCardList);

            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }

        public static void ToggleSortMode(SortMode requestedMode)
        {
            bool isToggleGroup = requestedMode switch
            {
                SortMode.LevelAsc or SortMode.LevelDesc => currentSortMode == SortMode.LevelAsc || currentSortMode == SortMode.LevelDesc,
                SortMode.CardIDAsc or SortMode.CardIDDesc => currentSortMode == SortMode.CardIDAsc || currentSortMode == SortMode.CardIDDesc,

                SortMode.CostAsc or SortMode.CostDesc => currentSortMode == SortMode.CostAsc || currentSortMode == SortMode.CostDesc,
                SortMode.StageAsc or SortMode.StageDesc => currentSortMode == SortMode.StageAsc || currentSortMode == SortMode.StageDesc,
                SortMode.QuantityAsc or SortMode.QuantityDesc => currentSortMode == SortMode.QuantityAsc || currentSortMode == SortMode.QuantityDesc,
                SortMode.PowerAsc or SortMode.PowerDesc => currentSortMode == SortMode.PowerAsc || currentSortMode == SortMode.PowerDesc,
                SortMode.NameAZ or SortMode.NameZA => currentSortMode == SortMode.NameAZ || currentSortMode == SortMode.NameZA,
                SortMode.TypeMode1 or SortMode.TypeMode2 or SortMode.TypeMode3 =>
                    currentSortMode == SortMode.TypeMode1 || currentSortMode == SortMode.TypeMode2 || currentSortMode == SortMode.TypeMode3,
                _ => false,
            };

            if (isToggleGroup && requestedMode == currentSortMode)
            {
                currentSortMode = requestedMode switch
                {
                    SortMode.LevelAsc => SortMode.LevelDesc,
                    SortMode.LevelDesc => SortMode.LevelAsc,
                    SortMode.CostAsc => SortMode.CostDesc,
                    SortMode.CostDesc => SortMode.CostAsc,
                    SortMode.StageAsc => SortMode.StageDesc,
                    SortMode.StageDesc => SortMode.StageAsc,
                    SortMode.QuantityAsc => SortMode.QuantityDesc,
                    SortMode.QuantityDesc => SortMode.QuantityAsc,
                    SortMode.PowerAsc => SortMode.PowerDesc,
                    SortMode.PowerDesc => SortMode.PowerAsc,
                    SortMode.NameAZ => SortMode.NameZA,
                    SortMode.NameZA => SortMode.NameAZ,
                    SortMode.TypeMode1 => SortMode.TypeMode2,
                    SortMode.TypeMode2 => SortMode.TypeMode3,
                    SortMode.TypeMode3 => SortMode.TypeMode1,
                    SortMode.CardIDAsc => SortMode.CardIDDesc,
                    SortMode.CardIDDesc => SortMode.CardIDAsc,
                  
                    

                    _ => currentSortMode
                };
            }
            else
            {
                previousSortMode = currentSortMode;
                currentSortMode = requestedMode;
            }
        }


        public static void SortCards(List<Card> cards)
        {
            if (currentSortMode == SortMode.ViewDeckOrder)
            {
                cards.Sort((a, b) => CompareBySortMode(a, b, SortMode.ViewDeckOrder));
                return;
            }
            cards.Sort((a, b) =>
            {
                int primaryComparison = CompareBySortMode(a, b, currentSortMode);
                if (primaryComparison != 0)
                    return primaryComparison;

                // Evita qualquer interferência quando o modo atual é ViewDeckOrder
                if (currentSortMode != SortMode.ViewDeckOrder &&
                    previousSortMode.HasValue &&
                    previousSortMode.Value != currentSortMode &&
                    previousSortMode.Value != SortMode.Default &&
                    currentSortMode != SortMode.Default)
                {
                    int secondaryComparison = CompareBySortMode(a, b, previousSortMode.Value);
                    if (secondaryComparison != 0)
                        return secondaryComparison;
                }


                return string.Compare(a.cardName, b.cardName);
            });
        }


        private static int CompareBySortMode(Card a, Card b, SortMode mode)
        {
            Debug.Log($"[CompareBySortMode] SortMode em uso: {mode}");
            bool aIsDigimonOrPartner = a.cardType == CardType.Digimon || a.cardType == CardType.Partner;
            bool bIsDigimonOrPartner = b.cardType == CardType.Digimon || b.cardType == CardType.Partner;
            switch (mode)
            {
                case SortMode.ViewDeckOrder:
                    {
                        int GetGroupOrder(Card card) => card.cardType switch
                        {
                            CardType.Partner => 0,
                            CardType.Skill => 1,
                            CardType.Digimon => 2,
                            CardType.Program => 3,
                            _ => 4,
                        };

                        int groupA = GetGroupOrder(a);
                        int groupB = GetGroupOrder(b);
                        if (groupA != groupB)
                            return groupA.CompareTo(groupB);

                        // Agrupar cópias juntas pelo cardID
                        int idCompare = string.Compare(a.cardID, b.cardID, StringComparison.Ordinal);
                        if (idCompare != 0)
                            return idCompare;

                        // Ordem decrescente dentro do grupo
                        return groupA switch
                        {
                            0 => ((b as DigimonCard)?.level ?? int.MinValue).CompareTo((a as DigimonCard)?.level ?? int.MinValue), // Partner decrescente por level
                            1 => GetCost(b).CompareTo(GetCost(a)),   // Skill decrescente por Cost
                            2 => ((b as DigimonCard)?.level ?? int.MinValue).CompareTo((a as DigimonCard)?.level ?? int.MinValue), // Digimon decrescente por level
                            3 => GetCost(b).CompareTo(GetCost(a)),   // Program decrescente por Cost
                            _ => 0,
                        };
                    }



                case SortMode.TypeDigimon:
                    {
                        int GetGroupOrder(Card card)
                        {
                            return card.cardType switch
                            {
                                CardType.Digimon => 0,
                                CardType.Partner => 0,
                                CardType.Program => 1,
                                CardType.Skill => 2,
                                _ => 3
                            };
                        }

                        int groupA = GetGroupOrder(a);
                        int groupB = GetGroupOrder(b);

                        if (groupA != groupB)
                            return groupA.CompareTo(groupB);

                        // Dentro do grupo 0 (Digimon/Partner), ordenar por type
                        if (groupA == 0)
                        {
                            DigimonType aType = (a as DigimonCard).type;
                            DigimonType bType = (b as DigimonCard).type;
                            return aType.CompareTo(bType);
                        }

                        // Fora do grupo Digimon/Partner, manter ordem padrão
                        return 0;
                    }
                case SortMode.LevelAsc:
                    {
                        int aLevel = GetLevel(a);
                        int bLevel = GetLevel(b);

                        // Priorizar Digimon e Partner com nível (level > 0)
                        bool aHasLevel = aIsDigimonOrPartner && aLevel > 0;
                        bool bHasLevel = bIsDigimonOrPartner && bLevel > 0;

                        if (aHasLevel && !bHasLevel) return -1;
                        if (!aHasLevel && bHasLevel) return 1;
                        if (aHasLevel && bHasLevel) return aLevel.CompareTo(bLevel);

                        // Se nenhum dos dois tem level, mantém ordem natural
                        return 0;
                    }
                case SortMode.LevelDesc:
                    {
                        int aLevel = GetLevel(a);
                        int bLevel = GetLevel(b);

                        bool aHasLevel = aIsDigimonOrPartner && aLevel > 0;
                        bool bHasLevel = bIsDigimonOrPartner && bLevel > 0;

                        if (aHasLevel && !bHasLevel) return -1;
                        if (!aHasLevel && bHasLevel) return 1;
                        if (aHasLevel && bHasLevel) return bLevel.CompareTo(aLevel);

                        return 0;
                    }
                case SortMode.CostAsc:
                    {
                        int aCost = GetCost(a);
                        int bCost = GetCost(b);

                        bool aHasCost = aCost > 0;
                        bool bHasCost = bCost > 0;

                        if (aHasCost && !bHasCost) return -1;
                        if (!aHasCost && bHasCost) return 1;
                        if (aHasCost && bHasCost) return aCost.CompareTo(bCost);

                        return 0;
                    }
                case SortMode.CostDesc:
                    {
                        int aCost = GetCost(a);
                        int bCost = GetCost(b);

                        bool aHasCost = aCost > 0;
                        bool bHasCost = bCost > 0;

                        if (aHasCost && !bHasCost) return -1;
                        if (!aHasCost && bHasCost) return 1;
                        if (aHasCost && bHasCost) return bCost.CompareTo(aCost);

                        return 0;
                    }
                case SortMode.StageAsc:
                    {
                        int aStage = GetStage(a);
                        int bStage = GetStage(b);

                        bool aHasStage = aIsDigimonOrPartner && aStage > 0;
                        bool bHasStage = bIsDigimonOrPartner && bStage > 0;

                        if (aHasStage && !bHasStage) return -1;
                        if (!aHasStage && bHasStage) return 1;
                        if (aHasStage && bHasStage) return aStage.CompareTo(bStage);

                        return 0;
                    }
                case SortMode.StageDesc:
                    {
                        int aStage = GetStage(a);
                        int bStage = GetStage(b);

                        bool aHasStage = aIsDigimonOrPartner && aStage > 0;
                        bool bHasStage = bIsDigimonOrPartner && bStage > 0;

                        if (aHasStage && !bHasStage) return -1;
                        if (!aHasStage && bHasStage) return 1;
                        if (aHasStage && bHasStage) return bStage.CompareTo(aStage);

                        return 0;
                    }
                case SortMode.PowerAsc:
                    {
                        int aPower = GetPower(a);
                        int bPower = GetPower(b);

                        bool aHasPower = aPower > 0;
                        bool bHasPower = bPower > 0;

                        if (aHasPower && !bHasPower) return -1;
                        if (!aHasPower && bHasPower) return 1;
                        if (aHasPower && bHasPower) return aPower.CompareTo(bPower);

                        return 0;
                    }
                case SortMode.PowerDesc:
                    {
                        int aPower = GetPower(a);
                        int bPower = GetPower(b);

                        bool aHasPower = aPower > 0;
                        bool bHasPower = bPower > 0;

                        if (aHasPower && !bHasPower) return -1;
                        if (!aHasPower && bHasPower) return 1;
                        if (aHasPower && bHasPower) return bPower.CompareTo(aPower);

                        return 0;
                    }
                case SortMode.Attribute:
                    {
                        // Agrupamento: Digimon/Partner (0), Program (1), Skill (2), outros (3)
                        int GetGroupOrder(Card card) => card.cardType switch
                        {
                            CardType.Digimon => 0,
                            CardType.Partner => 0,
                            CardType.Program => 1,
                            CardType.Skill => 2,
                            _ => 3,
                        };

                        int groupA = GetGroupOrder(a);
                        int groupB = GetGroupOrder(b);

                        if (groupA != groupB)
                            return groupA.CompareTo(groupB);

                        // Dentro Digimon/Partner, ordena por atributo
                        if (groupA == 0)
                        {
                            bool aHasAttr = (a is DigimonCard digA) && digA.attribute != 0;
                            bool bHasAttr = (b is DigimonCard digB) && digB.attribute != 0;

                            if (aHasAttr && !bHasAttr) return -1;
                            if (!aHasAttr && bHasAttr) return 1;

                            if (aHasAttr && bHasAttr)
                            {
                                int attrA = (a as DigimonCard).attribute.GetHashCode();
                                int attrB = (b as DigimonCard).attribute.GetHashCode();
                                return attrA.CompareTo(attrB);
                            }
                        }

                        return 0;
                    }
                case SortMode.Field:
                    {
                        int GetFieldOrder(Card card)
                        {
                            return card.cardType switch
                            {
                                CardType.Digimon => 0,
                                CardType.Partner => 1,
                                CardType.Program => 2,
                                CardType.Skill => 3,
                                _ => 4,
                            };
                        }

                        int groupOrderA = GetFieldOrder(a);
                        int groupOrderB = GetFieldOrder(b);
                        if (groupOrderA != groupOrderB)
                            return groupOrderA.CompareTo(groupOrderB);

                        return GetField(a).CompareTo(GetField(b));
                    }
                case SortMode.TypeMode1:
                    return GetTypeOrder(a, SortMode.TypeMode1).CompareTo(GetTypeOrder(b, SortMode.TypeMode1));
                case SortMode.TypeMode2:
                    return GetTypeOrder(a, SortMode.TypeMode2).CompareTo(GetTypeOrder(b, SortMode.TypeMode2));
                case SortMode.TypeMode3:
                    return GetTypeOrder(a, SortMode.TypeMode3).CompareTo(GetTypeOrder(b, SortMode.TypeMode3));
                case SortMode.NameAZ:
                    return string.Compare(a.cardName, b.cardName);
                case SortMode.NameZA:
                    return string.Compare(b.cardName, a.cardName);
                case SortMode.Color:
                    return a.cardColor.FirstOrDefault().CompareTo(b.cardColor.FirstOrDefault());
                
                case SortMode.QuantityAsc:
                    return GetCardCount(a.cardID).CompareTo(GetCardCount(b.cardID));
                case SortMode.QuantityDesc:
                    return GetCardCount(b.cardID).CompareTo(GetCardCount(a.cardID));
                case SortMode.CardIDAsc:
                    return string.Compare(a.cardID, b.cardID);
                case SortMode.CardIDDesc:
                    return string.Compare(b.cardID, a.cardID);
                
                case SortMode.Default:
                default:
                    return 0;
            }
        }
        private static int GetPower(Card card)
        {
            if (card is DigimonCard digimon)
                return digimon.power;

            return 0;
        }
        private static int GetAttributeOrder(Card card)
        {
            if (card is DigimonCard digimon)
                return (int)digimon.attribute; // Supondo que Attribute é um enum
            return -1; // Para cards sem atributo definido, ficam por último
        }
        private static DigimonField GetField(Card card)
        {
            if (card is DigimonCard digimon)
                return digimon.fieldDigimon;

            return DigimonField.NoField; // Assumindo que 'None' representa ausência de campo
        }
        private static int GetCardCount(string cardID)
        {
            if (string.IsNullOrEmpty(cardID) || cardQuantities == null)
                return 0;

            return cardQuantities.TryGetValue(cardID, out int count) ? count : 0;
        }

        private static int GetLevel(Card card)
        {
            if (card is DigimonCard digimon)
                return digimon.level;
            if (card.cardType == CardType.Partner)
                return 1000;
            return 0;
        }

        private static int GetCost(Card card)
        {
            return card.cost?.Sum() ?? 0;
        }

        private static int GetStage(Card card)
        {
            if (card is DigimonCard digimon)
                return (int)digimon.stage;
            return 0;
        }

        private static int GetTypeOrder(Card card, SortMode mode)
        {
            return mode switch
            {
                SortMode.TypeMode1 => card.cardType switch
                {
                    CardType.Digimon => 0,
                    CardType.Partner => 1,
                    CardType.Program => 2,
                    CardType.Skill => 3,
                    _ => 4,
                },
                SortMode.TypeMode2 => card.cardType switch
                {
                    CardType.Partner => 0,
                    CardType.Skill => 1,
                    CardType.Digimon => 2,
                    CardType.Program => 3,
                    _ => 4,
                },
                SortMode.TypeMode3 => card.cardType switch
                {
                    CardType.Digimon => 0,
                    CardType.Partner => 1,
                    CardType.Program => 2,
                    CardType.Skill => 3,
                    _ => 4,
                },
                _ => 4,
            };
        }

        public static void SortCardsDefault(List<Card> cards)
        {
            int GetCardTypeOrder(CardType type)
            {
                return CardTypeOrder.TryGetValue(type, out int order) ? order : 99;
            }

            cards.Sort((a, b) =>
            {
                int typeOrderA = GetCardTypeOrder(a.cardType);
                int typeOrderB = GetCardTypeOrder(b.cardType);

                if (typeOrderA != typeOrderB)
                    return typeOrderA.CompareTo(typeOrderB);

                int secondaryComparison = 0;
                if (a.cardType == CardType.Digimon || a.cardType == CardType.Partner)
                {
                    int levelA = (a is DigimonCard digA) ? digA.level : 0;
                    int levelB = (b is DigimonCard digB) ? digB.level : 0;
                    secondaryComparison = levelB.CompareTo(levelA);
                }
                else if (a.cardType == CardType.Program || a.cardType == CardType.Skill)
                {
                    int costA = a.cost?.Sum() ?? 0;
                    int costB = b.cost?.Sum() ?? 0;
                    secondaryComparison = costB.CompareTo(costA);
                }

                if (secondaryComparison != 0)
                    return secondaryComparison;

                int nameComparison = string.Compare(a.cardName, b.cardName);
                if (nameComparison != 0)
                    return nameComparison;

                return string.Compare(a.cardID, b.cardID);
            });
        }
        public static void UpdateCardQuantitiesFromDeck(List<DeckCardEntry> mainDeck, List<DeckCardEntry> partnerDeck)
        {
            cardQuantities = new Dictionary<string, int>();

            void AddQuantities(List<DeckCardEntry> deck)
            {
                foreach (var entry in deck)
                {
                    if (cardQuantities.ContainsKey(entry.cardID))
                        cardQuantities[entry.cardID] += entry.quantity;
                    else
                        cardQuantities[entry.cardID] = entry.quantity;
                }
            }

            AddQuantities(mainDeck);
            AddQuantities(partnerDeck);
        }
        public static SortMode GetCurrentSortMode()
        {
            return currentSortMode;
        }

        public static SortMode? GetPreviousSortMode()
        {
            return previousSortMode;
        }
    }
}
