using ProjectScript.Enums;
using System.Collections.Generic;

namespace ProjectScript.Selection
{
    public class SelectionCriteria
    {
        public FieldPlace? placeRequirements;
        public PlayerSide? sideRequirements;
        public CardType? typeRequirements;
        public DigimonField? fieldRequirements;

        public Dictionary<CardColor, int> colorRequirements;
    }
}
