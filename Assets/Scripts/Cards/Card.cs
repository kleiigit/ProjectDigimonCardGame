using UnityEngine;
using System.Collections.Generic;
using ProjectScript.Enums;

namespace SinuousProductions
{

    public class Card : ScriptableObject
    {
        [Header("Basic Information:")]
        public CardType cardType; // Changed to a single CardType instance
        [HideInInspector]
        public string cardID;
        public string cardName;
        public CardRarity cardRarity;
        public List<CardColor> cardColor;

        public List<CardColor> costColor;
        public List<int> cost;
        public List<DigimonCrest> limitDeck;

        [Header("Sprite Configuration:")]
        public Sprite sprite;

        [Header("Card Description and Keyword:")]
        public List<CardEffects> effects = new List<CardEffects>(); // List of Effect instances for card effects


        #region Enumeration
        
        public enum SkillActivation
        {
            MainPhase,
            AttackPhase,
            AntiProgram,
            MainPhaseAndBattlePhase,
            AllTime
        }
        #endregion
        private void OnValidate()
        {
            // Atualiza o cardID para o nome do asset, impedindo edição manual
            if (cardID != name)
            {
                cardID = name;
                // Opcional: atualize também cardName para o nome padrão
                if (string.IsNullOrEmpty(cardName))
                    cardName = name;
            }
        }
        public Dictionary<CardColor,int> GetColorCost()
        {
            Dictionary<CardColor, int> cardCostColor = new Dictionary<CardColor, int>();
            for(int i = 0; i < costColor.Count; i++)
            {
                int value = (i < cost.Count) ? cost[i] : 0;
                cardCostColor[costColor[i]] = value;
            }
            return cardCostColor;
        }
    }
}