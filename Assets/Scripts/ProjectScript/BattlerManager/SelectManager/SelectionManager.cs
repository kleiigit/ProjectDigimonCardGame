using ProjectScript.Enums;
using ProjectScript.Interfaces;
using ProjectScript.Selection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    private SelectionRequest currentRequest;
    private readonly List<ISelectable> selected = new();

    public bool IsSelecting => currentRequest != null;
    public IReadOnlyList<ISelectable> CurrentSelection => selected;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartSelection(SelectionRequest request, RectTransform canvas)
    {
        currentRequest = request;
        ClearSelection();

        UIWindowManager.Instance.ShowSelectionModal(
            canvas,
            "Pagamento de custo",
            "Selecione as cartas necessárias",
            request.amount,
            () => selected
                .Select(s => ((MonoBehaviour)s).gameObject)
                .ToList(),
            () =>
            {
                ConfirmSelection();
            }
        );
    }


    public void TrySelect(ISelectable selectable)
    {
        if (currentRequest == null || selectable == null)
            return;

        // PRIORIDADE: se já estiver selecionado, sempre permitir deselecionar
        if (selected.Contains(selectable))
        {
            selected.Remove(selectable);
            selectable.OnDeselected();
            return;
        }

        // Validação só para novas seleções
        if (!IsValidSelectable(selectable, currentRequest.criteria))
            return;

        if (selected.Count >= currentRequest.amount)
            return;

        selected.Add(selectable);
        selectable.OnSelected();
    }
    private bool IsValidSelectable(ISelectable selectable, SelectionCriteria criteria)
    {
        if (criteria == null)
            return true;

        if (selectable is not MonoBehaviour mono)
            return false;

       if (!mono.TryGetComponent<CardDisplay>(out var card))
             return false;

        if (criteria.placeRequirements.HasValue && !PlaceRequirements(criteria.placeRequirements, card))
        {
            return false;
        }
        if (criteria.colorRequirements != null && criteria.colorRequirements.Count > 0)
        {
            Dictionary<CardColor, int> currentColors = GetCurrentColorSelection();

            if (!ColorRequirements(
                    card.cardData.cardColor[0],
                    criteria,
                    currentColors))
                return false;
        }
        if (criteria.typeRequirements.HasValue &&
            card.cardData.cardType != criteria.typeRequirements.Value)
            return false;

        DigimonCard digimonCard = card.cardData as DigimonCard;
        if (criteria.fieldRequirements.HasValue &&
            digimonCard.fieldDigimon != criteria.fieldRequirements.Value)
            return false;

        return true;
    }
    bool PlaceRequirements(FieldPlace? fieldPlace, CardDisplay card)
    {
        int layerMask = fieldPlace switch
        {
            FieldPlace.Hand => 10,
            FieldPlace.DataPile => 12,
            FieldPlace.BattleZone => 7,
            FieldPlace.SecurityPile => 13,
            FieldPlace.TrashPile => 15,
            _ => card.gameObject.layer
        };

        return card.gameObject.layer == layerMask;
    }
    bool ColorRequirements(CardColor color, SelectionCriteria criteria, Dictionary<CardColor, int> current)
    {
        int totalRequired = criteria.colorRequirements.Values.Sum();
        int totalSelected = current.Values.Sum();

        // Nunca ultrapassar o custo total
        if (totalSelected >= totalRequired)
            return false;

        // COLORLESS sempre pode pagar qualquer custo pendente
        if (color == CardColor.Colorless)
        {
            foreach (var requirement in criteria.colorRequirements)
            {
                current.TryGetValue(requirement.Key, out int paid);

                if (paid < requirement.Value)
                    return true;
            }

            return false;
        }

        // Se a cor NÃO está no custo, ela não pode ser usada
        if (!criteria.colorRequirements.ContainsKey(color))
            return false;

        // Para cores exigidas
        current.TryGetValue(color, out int currentAmount);
        int requiredOfColor = criteria.colorRequirements[color];

        // Ainda falta pagar essa cor?
        if (currentAmount < requiredOfColor)
            return true;

        // Essa cor já foi paga; verificar se pode preencher colorless
        criteria.colorRequirements.TryGetValue(CardColor.Colorless, out int colorlessRequired);
        current.TryGetValue(CardColor.Colorless, out int colorlessPaid);

        return colorlessPaid < colorlessRequired;
    }
    private Dictionary<CardColor, int> GetCurrentColorSelection()
    {
        Dictionary<CardColor, int> result = new();

        foreach (var selectable in selected)
        {
            if (selectable is not MonoBehaviour mono)
                continue;

            if (!mono.TryGetComponent<CardDisplay>(out var card))
                continue;

            CardColor color = card.cardData.cardColor[0];

            if (!result.ContainsKey(color))
                result[color] = 0;

            result[color]++;
        }

        return result;
    }

    public void ConfirmSelection()
    {
        if (currentRequest == null)
            return;

        currentRequest.onComplete?.Invoke(new List<ISelectable>(selected));

        currentRequest = null;
        ClearSelection();
    }

    public void CancelSelection()
    {
        ClearSelection();
        currentRequest = null;
    }

    private void ClearSelection()
    {
        foreach (var selectable in selected)
        {
            selectable.OnDeselected();
        }

        selected.Clear();
    }

    // Utilitário opcional
    public void DeselectAllByTag(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag);

        foreach (var obj in objects)
        {
            if (obj.TryGetComponent<ISelectable>(out var selectable))
            {
                selectable.OnDeselected();
            }
        }
    }
}
