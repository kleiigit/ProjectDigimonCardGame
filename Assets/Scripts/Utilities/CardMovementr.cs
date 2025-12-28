using UnityEngine;
using UnityEngine.EventSystems;
using ProjectScript.Enums;

public class CardMovementr : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    [SerializeField] private int currentState;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private GridManager gridManager;

    [SerializeField] private GameObject glowEffect;

    private LayerMask gridLayerMask;
    private LayerMask checkzoneLayerMask;

    private CardDisplay cardDisplay;
    private HandManager handManager;
    private DiscardManager discardManager;
    private int originalSiblingIndex;

    private bool isPointerOver = false;

    [Tooltip("Indica se esta carta pertence à mão 'Blue' ou 'Red'")]
    public PlayerSide handSide;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.localPosition;
        originalRotation = rectTransform.localRotation;

        gridManager = FindFirstObjectByType<GridManager>();
        handManager = FindFirstObjectByType<HandManager>();
        discardManager = FindFirstObjectByType<DiscardManager>();
        cardDisplay = GetComponent<CardDisplay>();

        gridLayerMask = LayerMask.GetMask("FieldGrid");
        checkzoneLayerMask = LayerMask.GetMask("CheckZone");
    }

    void Update()
    {
        switch (currentState)
        {
            case 1:
                HandleHoverState();
                break;
            case 2:
                HandleDragState();
                if (!Input.GetMouseButton(0))
                {
                    TryPlayCardUnderCursor();

                    // Verifica se o mouse ainda está sobre a carta
                    if (!Input.GetMouseButton(0))
                    {
                        TryPlayCardUnderCursor();

                        if (isPointerOver)
                        {
                            EnterHoverState();
                        }
                        else
                        {
                            TransitionToState0();
                        }
                    }
                }
                break;
        }
    }

    private void TransitionToState0()
    {
        currentState = 0;
        rectTransform.localPosition = originalPosition;
        rectTransform.localRotation = originalRotation;
        transform.SetSiblingIndex(originalSiblingIndex);
        glowEffect.SetActive(false);
    }

    private void EnterHoverState()
    {
        originalPosition = rectTransform.localPosition;
        originalRotation = rectTransform.localRotation;

        // Salva a posição original na hierarquia
        originalSiblingIndex = transform.GetSiblingIndex();

        // Move para o topo da hierarquia (frente)
        transform.SetAsLastSibling();

        currentState = 1;
        HandleHoverState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        if (currentState == 0)
        {
            EnterHoverState();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;

        if (currentState == 1)
        {
            TransitionToState0();
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
        if (currentState == 1)
        {
            Debug.Log("Changing to Drag State");
            currentState = 2;
            Quaternion previousRotation = rectTransform.localRotation;
            rectTransform.localRotation = Quaternion.identity;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out originalLocalPointerPosition
            );

            originalPanelLocalPosition = rectTransform.localPosition;
            rectTransform.localRotation = previousRotation;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging...");
        if (currentState == 2)
        {
            Debug.Log("OnPointerDown called");
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.GetComponent<RectTransform>(),
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPointerPosition))
            {
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                rectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
            }
        }
    }

    private void HandleHoverState()
    {
        glowEffect.SetActive(true);
    }

    private void HandleDragState()
    {
        rectTransform.localRotation = Quaternion.identity;
    }

    private void TryPlayCardUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if ((int)cardDisplay.cardData.cardType == 0)
        {
            TryToPlayDigimonCard(ray);
        }
        else if ((int)cardDisplay.cardData.cardType == 1)
        {
            TryToPlayProgramCard(ray);
        }
    }

    private void TryToPlayDigimonCard(Ray ray)
    {
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, gridLayerMask);

        if (hit.collider != null && hit.collider.TryGetComponent<GridCell>(out var cell))
        {
            if (!IsValidGridForHandSide(cell.owner))
            {
                Debug.LogWarning($"Jogada inválida: carta {handSide} não pode ser jogada no grid de {cell.owner}");
                return;
            }

            int targetPos = cell.gridIndex;
            if (gridManager.AddObjectToGrid(cardDisplay.cardData, targetPos))
            {
                //discardManager.AddToDiscard(cardDisplay.cardData);
                RemoveCardFromHand();
                Debug.Log(cardDisplay.cardData.cardName.ToUpper() + " added to grid at position: " + targetPos);
                Destroy(gameObject);
            }
        }
    }

    public void TryToPlayProgramCard(Ray ray)
    {
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, checkzoneLayerMask);

        if (hit.collider != null && hit.collider.TryGetComponent<GridCell>(out var cell))
        {
            if (!IsValidGridForHandSide(cell.owner))
            {
                Debug.LogWarning($"Jogada inválida: carta {handSide} não pode ser jogada no grid de {cell.owner}");
                return;
            }

            int targetPos = cell.gridIndex;
            if (gridManager.AddObjectToGrid(cardDisplay.cardData, targetPos))
            {
                //discardManager.AddToDiscard(cardDisplay.cardData);
                RemoveCardFromHand();
                Debug.Log("Played Program: " + cardDisplay.cardData.cardName.ToUpper());
                Destroy(gameObject);
            }
        }
    }

    private void RemoveCardFromHand()
    {
            handManager.RemoveCard(gameObject);
    }

    private bool IsValidGridForHandSide(PlayerSide gridOwner)
    {
        return (handSide == PlayerSide.PlayerBlue && gridOwner == PlayerSide.PlayerBlue) ||
               (handSide == PlayerSide.PlayerRed && gridOwner == PlayerSide.PlayerRed);
    }
}
