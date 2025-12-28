using SinuousProductions;
using System.Globalization;
using System.Text;
using UnityEngine;
using ProjectScript.Enums;

[System.Serializable]
public class TargetCriteria
{
    public int count = 1;
    public FieldPlace placeCondition;
    public bool targetOwner; // true = jogador, false = oponente
    public RequiredCard requiredCardCondition;

    public bool Matches(FieldCard card)
    {
        if (card == null || card.digimonDisplay == null || card.digimonDisplay.digimonCardStartData == null)
        {
            Debug.LogWarning("TargetCriteria.Matches: card ou dados do Digimon estão nulos.");
            return false;
        }

        // Verifica localização via Layer
        CardLocation currentLocation = GetCardLocationByLayer(card.gameObject);
        if (requiredCardCondition.cardLocation != CardLocation.None && currentLocation != requiredCardCondition.cardLocation)
            return false;

        var cardData = card.digimonDisplay.digimonCardStartData;

        // Validação baseada na condição requerida
        bool conditionMatch = ValidateCardData(cardData);
        if (!conditionMatch) return false;

        // Verifica dono do campo
        GridCell gridCell = card.GetComponentInParent<GridCell>();
        if (gridCell == null)
        {
            Debug.LogWarning("TargetCriteria.Matches: GridCell não encontrado no parent do card.");
            return false;
        }
        return Equals(gridCell.owner, targetOwner ? PlayerSide.PlayerBlue : PlayerSide.PlayerRed);
    }



    public bool MatchesHandCard(CardDisplay card)
    {
        if (card == null || card.cardData == null)
        {
            Debug.LogWarning("TargetCriteria.MatchesHandCard: card ou cardData está nulo.");
            return false;
        }

        return ValidateCardData(card.cardData);
    }

    private bool ValidateCardData(Card cardData)
    {
        if (requiredCardCondition == null) return true;

        DigimonCard digimon = cardData as DigimonCard;
        if (digimon == null) return false;

        // Tipo base
        if (requiredCardCondition.typeCard != CardType.Digimon && digimon.cardType != requiredCardCondition.typeCard)
            return false;

        // Cor
        if (requiredCardCondition.colorCard != CardColor.NoColor)
        {
            if (digimon.cardColor == null || !digimon.cardColor.Contains(requiredCardCondition.colorCard))
                return false;
        }

        // Campo específico
        if (requiredCardCondition.fieldDigimon != DigimonField.NoField && digimon.fieldDigimon != requiredCardCondition.fieldDigimon)
            return false;

        // Atributo
        if (requiredCardCondition.attriDigimon != DigimonAttribute.NoAttribute && digimon.attribute != requiredCardCondition.attriDigimon)
            return false;

        // Nome
        if (!string.IsNullOrEmpty(requiredCardCondition.nameCard) && digimon.cardName != requiredCardCondition.nameCard)
            return false;

        // Tipo do Digimon
        if (requiredCardCondition.typeDigimon != DigimonType.Lesser && digimon.type != requiredCardCondition.typeDigimon)
            return false;

        // level
        if (requiredCardCondition.levelDigimon > 0 && digimon.level != requiredCardCondition.levelDigimon)
            return false;

        // Poder
        if (requiredCardCondition.PowerDigimon > 0 && digimon.power != requiredCardCondition.PowerDigimon)
            return false;

        // ID
        if (!string.IsNullOrEmpty(requiredCardCondition.IDOfCard) && digimon.cardID != requiredCardCondition.IDOfCard)
            return false;

        return true;
    }

    private static string RemoveAccents(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private CardLocation GetCardLocationByLayer(GameObject cardObj)
    {
        int layer = cardObj.layer;
        string layerName = LayerMask.LayerToName(layer);

        switch (layerName)
        {
            case "Digimon":
                return CardLocation.Field;

            case "Card":
                return CardLocation.Hand;

            case "Data":
                return CardLocation.DataPile;

            case "Security":
                return CardLocation.SecurityPile;

            case "Trash":
                return CardLocation.Trash;

            default:
                return CardLocation.None;
        }
    }

}
