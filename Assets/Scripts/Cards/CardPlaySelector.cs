using ProjectScript.Enums;
using SinuousProductions;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardPlaySelector : MonoBehaviour
{
    private CardDisplay cardDisplay;
    [Header("Settings control for playcard")]
    public LayerMask checkzoneLayerMask;
    private bool isAwaitingTarget = false;
    private GameManager gameManager;
    private GridManager gridManager;
    private HandManager handManager;
    private MenuCardManager menuCardManager;

    //teste
    private DiscardManager discardManager;

    private Image[] imageRenderers;
    private Color[] originalColors;

    // Parâmetros de brilho pulsante
    [SerializeField] private Color pulseColorA = new Color(1f, 0.84f, 0f, 1f); // Dourado
    [SerializeField] private Color pulseColorB = new Color(1f, 1f, 0.6f, 1f);  // Amarelo claro
    [SerializeField] private float pulseSpeed = 2f;
    private bool isPulsing = false;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        gridManager = FindFirstObjectByType<GridManager>();
        handManager = FindFirstObjectByType<HandManager>();
        cardDisplay = GetComponent<CardDisplay>();
        discardManager = FindFirstObjectByType<DiscardManager>();
        menuCardManager = GetComponent<MenuCardManager>();



        imageRenderers = GetComponentsInChildren<Image>();
        originalColors = new Color[imageRenderers.Length];

        for (int i = 0; i < imageRenderers.Length; i++)
        {
            originalColors[i] = imageRenderers[i].color;
        }
    }

    private void Update()
    {
        if (isPulsing)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            Color currentColor = Color.Lerp(pulseColorA, pulseColorB, t);

            foreach (var image in imageRenderers)
            {
                image.color = currentColor;
            }
        }
    }

    public void StartCardPlacement()
    {
        Debug.Log("StartCardPlacement chamado");

        if (isAwaitingTarget) return;

        Card card = cardDisplay.cardData;

        isAwaitingTarget = true;
        isPulsing = true; // Inicia o brilho pulsante

        gameManager.StartCoroutine(WaitForGridClick(card, menuCardManager.handOwner));
    }
    // joga a carta
    private IEnumerator WaitForGridClick(Card card, PlayerSide playerSide)
    {
        Debug.Log("Selecione um local no campo para jogar a carta.");

        while (isAwaitingTarget)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, checkzoneLayerMask);

                if (hit.collider != null && hit.collider.TryGetComponent<GridCell>(out var cell))
                {
                    if (!IsValidGridForHandSide(cell.owner))
                    {
                        Debug.LogWarning($"Jogada inválida: {playerSide} não pode jogar no grid de {cell.owner}");
                        CancelPlacement(1);
                        yield break;
                    }

                    if (gridManager.AddObjectToGrid(card, cell.gridIndex))
                    {
                        handManager.RemoveCard(this.gameObject);
                        Debug.Log("Carta jogada: " + card.cardName.ToUpper());
                        isAwaitingTarget = false;
                        Destroy(gameObject);
                        if(playerSide == PlayerSide.PlayerBlue)
                        {
                            GameSetupStart.playerBlue.discard.AddCard(card);
                        }
                        else
                        {
                            GameSetupStart.playerRed.discard.AddCard(card);
                        }
                        yield break;
                    }
                    else
                    {
                        Debug.Log("Grid ocupado. Escolha outra célula.");
                        CancelPlacement(2);
                        yield break;
                    }
                }
                else
                {
                    Debug.Log("Clique fora do grid válido, cancelando.");
                    CancelPlacement(3);
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void CancelPlacement(int n)
    {
        isAwaitingTarget = false;
        isPulsing = false;

        for (int i = 0; i < imageRenderers.Length; i++)
        {
            imageRenderers[i].color = originalColors[i];
        }

        Debug.Log("Colocação da carta cancelada." + n);
    }

    private bool IsValidGridForHandSide(PlayerSide gridOwner)
    {
        return (menuCardManager.handOwner == PlayerSide.PlayerBlue && gridOwner == PlayerSide.PlayerBlue) ||
               (menuCardManager.handOwner == PlayerSide.PlayerRed && gridOwner == PlayerSide.PlayerRed);
    }
}
