using ProjectScript.Enums;
using ProjectScript.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSelectable : MonoBehaviour, ISelectable, IPointerClickHandler
{
    [Header("Controle de seleção da carta")]
    [SerializeField] private GameObject selectionIndicator;

    private MenuCardManager menuCardManager;
    private PlayerSetup setup;

    public bool cardSelected;

    private void Awake()
    {
        menuCardManager = GetComponent<MenuCardManager>();

        setup = menuCardManager.handOwner == PlayerSide.PlayerBlue
            ? GameSetupStart.playerBlue
            : GameSetupStart.playerRed;

        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    public void OnSelected()
    {
        cardSelected = true;
        selectionIndicator.SetActive(true);
    }

    public void OnDeselected()
    {
        cardSelected = false;
        selectionIndicator.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[CardSelectable] IsSelecting: {SelectionManager.Instance.IsSelecting}");
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        // PRIORIDADE ABSOLUTA: modo seleção
        if (SelectionManager.Instance != null && SelectionManager.Instance.IsSelecting)
        {
            SelectionManager.Instance.TrySelect(this);
            return;
        }

            setup = menuCardManager.handOwner == PlayerSide.PlayerBlue
                ? GameSetupStart.playerBlue
                : GameSetupStart.playerRed;

        if (setup != null)
        {
            if (!setup.isActivePlayer && gameObject.layer != 17 && gameObject.layer != 12)
                return;
        }

        OnLeftClick();
    }

    private void OnLeftClick()
    {
        menuCardManager.ButtonInteractive();
    }
}
