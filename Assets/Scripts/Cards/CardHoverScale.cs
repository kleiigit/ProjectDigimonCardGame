using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Hover ao passar o mouse ou controle do jogador
    private RectTransform rectTransform;
    private Vector3 baseScale;
    private bool isHovered = false;
    private int originalSiblingIndex;

    [SerializeField] private float scaleMultiplier = 1.1f;

    [Header("Configurações")]
    public bool enableBringToFront = false; // Controle público da opção
    public Button buttonInteractiveCard;
    private CardDisplay cardDisplay;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        cardDisplay = GetComponent<CardDisplay>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovered) return;
        if (gameObject.layer == 13) return; //for segurança, não aplicar hover em objetos que já estão no layer 13

        baseScale = rectTransform.localScale;
        rectTransform.localScale = baseScale * scaleMultiplier;

        if (enableBringToFront && gameObject.layer != 14) // no deck editor nao ativa vir para frente
        {
            rectTransform.SetAsLastSibling();
        }

        isHovered = true;

        if (cardDisplay != null && cardDisplay.cardData != null && HoverCardManager.Instance != null)
        {
            HoverCardManager.Instance.ShowCard(cardDisplay.cardData);
        }
        else
        {
            Debug.LogWarning("[CardHoverScale] Não foi possível exibir a carta no HoverPanel. Verifique se CardDisplay e cardData estão atribuídos.", gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovered) return;

        rectTransform.localScale = baseScale;

        if (enableBringToFront)
        {
            rectTransform.SetSiblingIndex(originalSiblingIndex);
        }

        isHovered = false;

        Transform child = transform.Find("MenuCard(Clone)"); // substitua pelo nome real do filho
        if (buttonInteractiveCard != null)
        {
            //Debug.LogError("[CardHoverScale] buttonInteractiveCard não está atribuído.", gameObject);
            return;
        }
        if(buttonInteractiveCard != null)
            buttonInteractiveCard.interactable = true;

        if (child != null)
        {
            Destroy(child.gameObject);
        }
        // --------------------------------------------------------

        HoverCardManager.Instance.ClearPanel();
    }

    public void ResetHoverState()
    {
        isHovered = false;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
    }
}
