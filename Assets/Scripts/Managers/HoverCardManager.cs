using SinuousProductions;
using TMPro;
using UnityEngine;
using System.Globalization;
using System.Linq;

public class HoverCardManager : MonoBehaviour
{
    // CardDisplayInfo Script
    public static HoverCardManager Instance;

    [Header("Referências")]
    [SerializeField] private GameObject hoverPanel;
    private GameObject cardDisplayPrefab;
    [Header("Componentes do Card Display")]
    [SerializeField] private TMP_Text nameCard;
    [SerializeField] private TMP_Text typeCard;
    [SerializeField] private TMP_Text cardID;
    [SerializeField] private TMP_Text cardColor;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text security;
    [SerializeField] private TMP_Text limitedCrest;
    [SerializeField] private TMP_Text rarity;
    [SerializeField] private TMP_Text owned;


    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text type;
    [SerializeField] private TMP_Text memory;
    [SerializeField] private TMP_Text attribute;
    [SerializeField] private TMP_Text stage;
    [SerializeField] private TMP_Text field;

    [SerializeField] private TMP_Text actionTime;
    [SerializeField] private TMP_Text costText;





    [Header("Configurações")]
    [SerializeField] private float hoverCardScale = 100f; // Escala ajustável via Inspector


    private GameObject currentCard;

    private void Awake()
    {
        cardDisplayPrefab = GameManager.cardPrefab;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowCard(Card cardData)
    {
        if (hoverPanel == null)
        {
            Debug.LogError("hoverPanel está null no HoverCardManager.");
            return;
        }
        if (cardDisplayPrefab == null)
        {
            Debug.LogError("cardDisplayPrefab está null no HoverCardManager.");
            return;
        }
        if (cardData == null)
        {
            Debug.LogError("cardData está null ao chamar ShowCard.");
            return;
        }

        ClearPanel();

        currentCard = Instantiate(cardDisplayPrefab, hoverPanel.transform);
        currentCard.transform.localScale = new Vector3(hoverCardScale, hoverCardScale, 1f);

        CardDisplay display = currentCard.GetComponent<CardDisplay>();
        if (display == null)
        {
            Debug.LogError("Prefab instanciado não possui componente CardDisplay.");
            return;
        }

        display.cardData = cardData;
        display.UpdateCardDisplay();

        foreach (var button in currentCard.GetComponentsInChildren<UnityEngine.UI.Button>())
        {
            button.interactable = false;
        }

        // Log das propriedades de display importantes
        

        nameCard.text = cardData.cardName != null ? cardData.cardName.ToUpper() : string.Empty;
        cardID.text = cardData.cardID.ToString();
        cardColor.text = cardData.cardColor != null ? string.Join(", ", cardData.cardColor.Select(c => c.ToString().ToUpper())) : string.Empty;
        typeCard.text = cardData.cardType.ToString().ToUpper();
        rarity.text = cardData.cardRarity.ToString().ToUpper();
        description.text = display.cardDescriptionText != null ? display.cardDescriptionText.text : string.Empty;
        security.text = null;
        if (owned != null && CardsCollectionManager.Instance != null && cardData != null)
        {
            owned.text = CardsCollectionManager.Instance.GetCardQuantity(cardData.cardID).ToString();
        }
        else
        {
            owned.gameObject.SetActive(false);
        }


        if (typeCard.text == "DIGIMON" || typeCard.text == "PARTNER")
        {
            actionTime.gameObject.SetActive(false);

            DigimonCard digimonCard = cardData as DigimonCard;
            if (digimonCard == null)
            {
                Debug.LogWarning("cardData não é DigimonCard válido.");
            }
            else
            {
                level.text = "Level: " + digimonCard.Level.ToString();
                type.text = "Type: " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(digimonCard.Type.ToString().ToLower());
                attribute.text = "Attrib. " + digimonCard.Attribute.ToString();
                stage.text = "Stage: " + digimonCard.Stage.ToString();
                field.text = "Field: " + digimonCard.Field.ToString();
                memory.text = "Memory: " + digimonCard.Memory.ToString();
            }
        }
        else if (typeCard.text == "SKILL")
        {
            actionTime.gameObject.SetActive(true);
            actionTime.text = display.activationTime != null ? display.activationTime.text.ToUpper() : string.Empty;
        }

        if (display.securityImage != null && display.securityImage.gameObject.activeSelf)
        {
            security.text = "Segurança: \n" + (display.cardSecurityText != null ? display.cardSecurityText.text : string.Empty);
        }
    }


    public void ClearPanel()
    {
        if (currentCard != null)
        {
            Destroy(currentCard);
            currentCard = null;
        }

        static void ClearText(TMP_Text textField)
        {
            if (textField != null)
                textField.text = string.Empty;
        }

        ClearText(nameCard);
        ClearText(cardID);
        ClearText(cardColor);
        ClearText(typeCard);
        ClearText(rarity);
        ClearText(description);
        ClearText(limitedCrest);
        ClearText(level);
        ClearText(type);
        ClearText(memory);
        ClearText(attribute);
        ClearText(stage);
        ClearText(field);
        ClearText(actionTime);
        ClearText(costText);
        ClearText(security);
        ClearText(owned);
    }


}
