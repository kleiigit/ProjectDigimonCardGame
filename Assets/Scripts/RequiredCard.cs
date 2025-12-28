using UnityEngine;
using ProjectScript.Enums;

namespace SinuousProductions
{
    [System.Serializable]
    public class RequiredCard
    {
        [Header("Filtro geral do tipo da carta")]
        public CardType typeCard = CardType.Digimon;

        [Header("Filtro de características do Digimon")]
        public CardColor colorCard = CardColor.NoColor;
        public DigimonField fieldDigimon = DigimonField.NoField;
        public DigimonAttribute attriDigimon = DigimonAttribute.NoAttribute;

        [Header("Filtro por nome, tipo, level, poder e ID")]
        public string nameCard = string.Empty;
        public DigimonType typeDigimon = DigimonType.Lesser;
        public int levelDigimon = 0;
        public int PowerDigimon = 0;
        public string IDOfCard = string.Empty;

        [Header("Comparações avançadas")]
        public bool compareLevel = false;
        public bool levelLessThanOrEqual = false;
        public bool levelGreaterThanOrEqual = false;

        public bool comparePower = false;
        public bool powerLessThanOrEqual = false;
        public bool powerGreaterThanOrEqual = false;

        public CardLocation cardLocation = CardLocation.None;
    }
    public enum CardLocation
    {
        None,
        Field,
        Hand,
        DataPile,
        SecurityPile,
        Deck,
        Trash
    }
}
