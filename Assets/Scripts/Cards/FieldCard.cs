using UnityEngine;
using UnityEngine.UI;
using ProjectScript.Enums;

public class FieldCard : MonoBehaviour
{
    public GridCell parentCell;
    public bool downPosition = false;
    public GameObject downButton;
    public DigimonDisplay digimonDisplay;
    //private EffectManager effectManager;
    private CardDisplay cardDisplay;
    private MenuCardManager menuCardManager;
    private TriggerCardAction triggerCardAction;

    public CardType cardType;
    public GameObject highLight;
    private Image highLightImage;
    public GameObject interations;

    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI selectionNumberText;

    private Image[] allImages;
    private Color[] originalColors;

    private void Awake()
    {
        triggerCardAction = FindFirstObjectByType<TriggerCardAction>();
        //effectManager = FindFirstObjectByType<EffectManager>();
        cardDisplay = GetComponent<CardDisplay>();
        if (highLight != null)
            highLightImage = highLight.GetComponent<Image>();

        if (highLightImage == null)
            Debug.LogWarning("highLight não possui componente Image no " + gameObject.name);

        // Captura todas as imagens da carta e salva suas cores originais
        allImages = GetComponentsInChildren<Image>(includeInactive: true);
        originalColors = new Color[allImages.Length];
        for (int i = 0; i < allImages.Length; i++)
        {
            originalColors[i] = allImages[i].color;
        }
    }

    private void OnEnable()
    {
        digimonDisplay = GetComponent<DigimonDisplay>();
        cardDisplay = GetComponent<CardDisplay>();

        if (cardDisplay == null)
        {
            Debug.LogError("CardDisplay component not found on this GameObject: " + gameObject.name);
            return;
        }

        if (cardDisplay.cardData == null)
        {
            Debug.LogError("cardData is null in CardDisplay on GameObject: " + gameObject.name);
            return;
        }

        digimonDisplay.digimonCardStartData = cardDisplay.cardData as DigimonCard;
        cardType = cardDisplay.cardData.cardType;
        
    }

    private void Start()
    {
        
        if (cardType == CardType.Digimon || cardType == CardType.Partner)
        {
            digimonDisplay.enabled = true;

            // Garante que o Digimon será incluído em AllDigimons
            if (!DigimonDisplay.AllDigimons.Contains(digimonDisplay))
                DigimonDisplay.AllDigimons.Add(digimonDisplay);
            cardDisplay.protectionBox.rectTransform.anchoredPosition = new Vector2(-0.9653f, 0.189389f);
            cardDisplay.protectionBox.rectTransform.localScale = new Vector3(1f, 1f, 1);
            triggerCardAction.TriggerOnplay(cardDisplay.cardData);

            if (cardType == CardType.Program)
            {
                Debug.Log("Carta Programa jogada! " + cardDisplay.cardData.cardName.ToUpper() + $" [{cardDisplay.cardData.effects.Count}]");
            }
        }
    }

    private void Update()
    {
        
        if (BattlePhaseManager.phase == Phase.UpPhase && downPosition)
        {
            
            if (menuCardManager.handOwner == BattlePhaseManager.currentPlayer)
            {
                Debug.Log(this.gameObject.name + " up!");
                downPosition = false;
            }
        }

        UpdatePosition();
        UpdateCardDarkness();
        UpdatePowerColor();

    }

    public void SetDownPosition()
    {
        downPosition = true;
        Debug.Log(this.gameObject.name + " down!");
    }
    public void DestroyFieldCard()
    {
        if (cardDisplay == null || cardDisplay.cardData == null)
        {
            Debug.LogWarning("DestroyCardAndSendToDataPile: cardDisplay ou cardData está nulo.");
            return;
        }

        PlayerSide playerSide = GetFieldOwner();

        GameManager.Instance.cardsInFieldBlue.Remove(this.gameObject);
        GameManager.Instance.cardsInFieldRed.Remove(this.gameObject);
        parentCell.cellFull = false;

        if (menuCardManager.handOwner == PlayerSide.PlayerBlue)
        {
            GameSetupStart.playerBlue.dataPile.AddCard(cardDisplay.cardData);
        }
        else
        {
            GameSetupStart.playerRed.dataPile.AddCard(cardDisplay.cardData);
        }
        Destroy(this.gameObject);
    }

    // Update Methods
    private void UpdatePosition()
    {
        if (downPosition)
        {
            this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            cardDisplay.PowerImage.rectTransform.localScale = new Vector3(1f, 1f, 1f);
            cardDisplay.cardPowerText.color = Color.grey;
        }
        else
        {
            this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            cardDisplay.PowerImage.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1f);
            cardDisplay.cardPowerText.color = Color.white;
        }
    }
    public void UpdatePowerColor()
    {
        cardDisplay.cardPowerText.color = (digimonDisplay.power - int.Parse(cardDisplay.cardPowerText.text)) switch
        {
            //Buff
            < 0 => Color.lightBlue,
            // Debuff
            > 0 => Color.red,
            _ => Color.white,
        };
    }
    public void UpdateCardDarkness()
    {
        for (int i = 0; i < allImages.Length; i++)
        {
            if (downPosition)
            {
                allImages[i].color = Color.Lerp(originalColors[i], Color.black, 0.5f);
            }
            else
            {
                allImages[i].color = originalColors[i];
            }
        }
    }

    #region(Highlight Methods)
    public void SetHighlightActive(bool state)
    {
        if (highLight != null)
            highLight.SetActive(state);
    }
    public PlayerSide GetFieldOwner()
    {
        return parentCell.owner;
    }
    public void SetCardOwnerFromGrid()
    {
        if (parentCell != null)
        {
            menuCardManager.handOwner = GetFieldOwner();
        }
        else
        {
            Debug.LogWarning($"[SetCardOwnerFromGrid] parentCell está nulo em {gameObject.name}.");
        }
    }
    public void SetHighlightColor(Color color)
    {
        if (highLightImage != null)
            highLightImage.color = color;
    }
    public void ShowSelectionIndicator(int order)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(true);
            if (selectionNumberText != null)
            {
                selectionNumberText.text = order.ToString();
            }
        }
    }
    public void HideSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }
    #endregion
}
