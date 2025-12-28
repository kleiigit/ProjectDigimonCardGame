using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SelectionContext
{
    Field,
    Hand
}

public class TargetSelector : MonoBehaviour
{
    [SerializeField] private GameObject selectionUIPanel;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMPro.TextMeshProUGUI instructionText;
    [SerializeField] private GraphicRaycaster handRaycaster;

    private int maxSelections;
    private List<FieldCard> selectedFieldCards = new List<FieldCard>();
    private List<GameObject> selectedHandCards = new List<GameObject>();
    private Action<List<FieldCard>> onFieldSelectionComplete;
    private Action<List<GameObject>> onHandSelectionComplete;
    private bool selectionActive = false;
    private bool awaitingConfirmation = false;
    private Coroutine pulseCoroutine = null;

    private FieldCard currentHoveredFieldCard = null;
    private GameObject currentHoveredHandCard = null;

    private int digimonLayerMask;
    [SerializeField] private LayerMask cardLayerMask;
    public EventSystem eventSystem;
    public GraphicRaycaster graphicRaycaster;

    private TargetCriteria criteria;
    private SelectionContext currentContext;

    // NOVO: Lista de alvos válidos para seleção
    private List<FieldCard> validFieldTargets = new List<FieldCard>();

    private void Awake()
    {
        digimonLayerMask = LayerMask.GetMask("Digimon");
        if (digimonLayerMask == 0)
        {
            Debug.LogError("Layer 'Digimon' não encontrada. Verifique se a layer existe e está escrita corretamente.");
        }

        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);

        confirmationPanel.SetActive(false);
        selectionUIPanel.SetActive(false);
    }

    private void Update()
    {
        if (!selectionActive || awaitingConfirmation)
            return;

        if (currentContext == SelectionContext.Field)
        {
            UpdateHoverField();

            if (Input.GetMouseButtonDown(0))
            {
                if (currentHoveredFieldCard != null && !selectedFieldCards.Contains(currentHoveredFieldCard))
                {
                    selectedFieldCards.Add(currentHoveredFieldCard);
                    currentHoveredFieldCard.ShowSelectionIndicator(selectedFieldCards.Count);
                    Debug.Log($"[TargetSelector] Alvo selecionado: {currentHoveredFieldCard.name} ({selectedFieldCards.Count}/{maxSelections})");

                    if (selectedFieldCards.Count >= maxSelections)
                    {
                        awaitingConfirmation = true;
                        confirmationPanel.SetActive(true);
                    }
                }
            }
        }
        else if (currentContext == SelectionContext.Hand)
        {
            UpdateHoverHand();

            if (Input.GetMouseButtonDown(0))
            {
                if (currentHoveredHandCard != null && !selectedHandCards.Contains(currentHoveredHandCard))
                {
                    selectedHandCards.Add(currentHoveredHandCard);
                    SetHoverIndicatorForHandCard(currentHoveredHandCard, true);
                    Debug.Log($"[TargetSelector] Carta da mão selecionada: {currentHoveredHandCard.name} ({selectedHandCards.Count}/{maxSelections})");

                    if (selectedHandCards.Count >= maxSelections)
                    {
                        awaitingConfirmation = true;
                        confirmationPanel.SetActive(true);
                    }
                }
            }
        }
    }

    // Novo método público para iniciar seleção filtrada de FieldCards
    public Coroutine SelectTargets(TargetCriteria selectionCriteria, List<FieldCard> validTargets, Action<List<FieldCard>> onComplete)
    {
        validFieldTargets = validTargets;
        return StartCoroutine(StartSelection(SelectionContext.Field, selectionCriteria.count, selectionCriteria, onComplete, null));
    }

    public IEnumerator StartSelection(SelectionContext context, int max, TargetCriteria selectionCriteria, Action<List<FieldCard>> fieldCallback, Action<List<GameObject>> handCallback)
    {
        maxSelections = max;
        criteria = selectionCriteria;
        currentContext = context;
        onFieldSelectionComplete = fieldCallback;
        onHandSelectionComplete = handCallback;

        StartSelection();

        while (selectionActive)
        {
            yield return null;
        }
    }

    private void StartSelection()
    {
        ResetSelections();

        selectionActive = true;
        awaitingConfirmation = false;

        selectionUIPanel.SetActive(true);
        confirmationPanel.SetActive(false);

        string criteriaInfo = "Critérios aplicados"; // Ajuste para exibir info real se desejar

        if (currentContext == SelectionContext.Field)
        {
            instructionText.text = $"Escolha {maxSelections} alvo(s) com critérios: {criteriaInfo}.";
            Debug.Log($"[TargetSelector] Iniciando seleção FIELD: escolha {maxSelections} alvo(s). Critérios: {criteriaInfo}.");
        }
        else
        {
            instructionText.text = $"Selecione até {maxSelections} carta(s) da mão para enviar ao DataPile.";
        }
    }

    private void OnConfirmClicked()
    {
        Debug.Log("[TargetSelector] Seleção confirmada pelo usuário.");
        FinalizeSelection();
    }

    private void OnCancelClicked()
    {
        Debug.Log("[TargetSelector] Seleção cancelada pelo usuário. Reiniciando seleção.");
        ResetSelections();
        StartSelection();
    }

    private void FinalizeSelection()
    {
        selectionActive = false;
        awaitingConfirmation = false;

        ClearHover();

        confirmationPanel.SetActive(false);
        selectionUIPanel.SetActive(false);

        if (currentContext == SelectionContext.Field)
        {
            Debug.Log($"[TargetSelector] Seleção FIELD encerrada. {selectedFieldCards.Count} alvo(s) selecionado(s).");
            onFieldSelectionComplete?.Invoke(new List<FieldCard>(selectedFieldCards));
        }
        else
        {
            Debug.Log($"[TargetSelector] Seleção HAND encerrada. {selectedHandCards.Count} carta(s) selecionada(s).");
            onHandSelectionComplete?.Invoke(new List<GameObject>(selectedHandCards));
        }

        ResetSelections();
        validFieldTargets.Clear();
    }

    private void ResetSelections()
    {
        if (currentContext == SelectionContext.Field)
        {
            foreach (FieldCard card in selectedFieldCards)
            {
                card.HideSelectionIndicator();
            }
            selectedFieldCards.Clear();
        }
        else
        {
            foreach (var card in selectedHandCards)
            {
                SetHoverIndicatorForHandCard(card, false);
            }
            selectedHandCards.Clear();
        }
    }

    private void UpdateHoverField()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 0f, digimonLayerMask);
        if (hit.collider != null)
        {
            FieldCard hitCard = hit.collider.GetComponent<FieldCard>();
            if (hitCard != null && validFieldTargets.Contains(hitCard) && !selectedFieldCards.Contains(hitCard))
            {
                if (currentHoveredFieldCard != hitCard)
                {
                    if (currentHoveredFieldCard != null)
                    {
                        Debug.Log($"[TargetSelector] Mouse saiu de: {currentHoveredFieldCard.name}");
                        SetHoverIndicator(currentHoveredFieldCard, false);
                    }

                    currentHoveredFieldCard = hitCard;
                    SetHoverIndicator(currentHoveredFieldCard, true);
                }
                return;
            }
        }

        if (currentHoveredFieldCard != null)
        {
            SetHoverIndicator(currentHoveredFieldCard, false);
            currentHoveredFieldCard = null;
        }
    }

    private void UpdateHoverHand()
    {
        if (handRaycaster == null || graphicRaycaster == null)
        {
            Debug.LogWarning("GraphicRaycaster ou Canvas da mão não estão atribuídos.");
            return;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        handRaycaster.Raycast(pointerData, results);

        GameObject hoveredCard = null;

        foreach (var result in results)
        {
            CardDisplay cardDisplay = result.gameObject.GetComponent<CardDisplay>();
            if (cardDisplay != null && !selectedHandCards.Contains(result.gameObject))
            {
                hoveredCard = result.gameObject;
                break;
            }
        }

        if (hoveredCard != currentHoveredHandCard)
        {
            if (currentHoveredHandCard != null)
                SetHoverIndicatorForHandCard(currentHoveredHandCard, false);

            currentHoveredHandCard = hoveredCard;

            if (currentHoveredHandCard != null)
                SetHoverIndicatorForHandCard(currentHoveredHandCard, true);
        }

        if (hoveredCard == null && currentHoveredHandCard != null)
        {
            SetHoverIndicatorForHandCard(currentHoveredHandCard, false);
            currentHoveredHandCard = null;
        }
    }

    private void ClearHover()
    {
        if (currentContext == SelectionContext.Field)
        {
            if (currentHoveredFieldCard != null)
            {
                SetHoverIndicator(currentHoveredFieldCard, false);
                currentHoveredFieldCard = null;
            }
        }
        else
        {
            if (currentHoveredHandCard != null)
            {
                SetHoverIndicatorForHandCard(currentHoveredHandCard, false);
                currentHoveredHandCard = null;
            }
        }
    }

    private void SetHoverIndicator(FieldCard card, bool active)
    {
        if (card == null) return;

        card.SetHighlightActive(active);

        if (active)
        {
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);

            pulseCoroutine = StartCoroutine(PulseHighlight(card));
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            card.SetHighlightColor(Color.white);
        }
    }

    private void SetHoverIndicatorForHandCard(GameObject cardObj, bool active)
    {
        if (cardObj == null) return;

        // Exemplo simples: ativar/desativar um componente Image para destacar a carta
        var image = cardObj.GetComponent<Image>();
        if (image != null)
        {
            image.color = active ? Color.yellow : Color.white;
        }

        // Se não quiser alterar cor, pode deixar vazio, ou implementar outro efeito conforme sua UI
    }



    private IEnumerator PulseHighlight(FieldCard card)
    {
        float pulseSpeed = 2f;
        Color baseColor = Color.yellow;
        Color transparentColor = new Color(1f, 1f, 0f, 0.3f);

        while (true)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            Color c = Color.Lerp(transparentColor, baseColor, t);
            card.SetHighlightColor(c);
            yield return null;
        }
    }

    // Método para detectar cartas UI na mão pelo cursor
    public GameObject DetectCardUnderCursor()
    {
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerData, results);

        foreach (var result in results)
        {
            GameObject go = result.gameObject;

            if (go.CompareTag("Card") && go.TryGetComponent<CardDisplay>(out var cardDisplay))
            {
                Debug.Log($"Carta UI detectada (Tag Card): {go.name}");
                return go;
            }
        }

        return null;
    }
}
